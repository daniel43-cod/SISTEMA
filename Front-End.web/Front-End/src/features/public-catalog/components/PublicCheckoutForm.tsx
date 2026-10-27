import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import type { CartItem } from '../types/product'
import { loadDeliveryZones, submitPublicOrder } from '../api/publicOrdersApi'
import type { DeliveryZone, PublicOrder } from '../api/publicOrdersApi'
import { createOperationId } from '../../sales/utils/operationId'
import { ApiError } from '../../../shared/api/ApiError'

const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
export function PublicCheckoutForm({ items, total, onBack, onBusy, onSuccess }: {
  items: CartItem[]; total: number; onBack: () => void; onBusy: (value: boolean) => void; onSuccess: (total: number) => void
}) {
  const [zones, setZones] = useState<DeliveryZone[]>([])
  const [loading, setLoading] = useState(true)
  const [zoneError, setZoneError] = useState('')
  const [revision, setRevision] = useState(0)
  const [zoneId, setZoneId] = useState('')
  const [name, setName] = useState('')
  const [phone, setPhone] = useState('')
  const [address, setAddress] = useState('')
  const [references, setReferences] = useState('')
  const [note, setNote] = useState('')
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [uncertain, setUncertain] = useState(false)
  const pending = useRef<PublicOrder | null>(null)
  const busy = useRef(false)
  const alive = useRef(true)
  const nameRef = useRef<HTMLInputElement>(null)
  useEffect(() => { alive.current = true; nameRef.current?.focus(); return () => { alive.current = false } }, [])
  useEffect(() => {
    const controller = new AbortController()
    setLoading(true); setZoneError('')
    void loadDeliveryZones(controller.signal).then(result => { if (!controller.signal.aborted) setZones(result) })
      .catch(failure => { if (!controller.signal.aborted) setZoneError(failure instanceof Error ? failure.message : 'No se pudieron cargar las zonas.') })
      .finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [revision])
  const zone = zones.find(z => z.idZonaEntrega === Number(zoneId))
  const enough = !!zone && Math.round(total * 100) > Math.round(zone.minimoCompra * 100)
  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (busy.current) return
    setError('')
    if (!uncertain) {
      if (!name.trim() || !address.trim() || !/^[0-9]{8}$/.test(phone) || !zone || !enough || !items.length || items.length > 100) {
        setError('Revisa tus datos, la zona, el mínimo de compra y los productos del pedido.'); return
      }
      try {
        pending.current = { idOperacion: createOperationId(), nombreCliente: name.trim(), telefono: '+502' + phone,
          direccion: address.trim(), referencias: references.trim(), observacion: note.trim(), idZonaEntrega: zone.idZonaEntrega,
          detalles: items.map(item => ({ idProductoPresentacion: item.idProductoPresentacion, cantidad: item.cantidad })) }
      } catch { setError('No se pudo preparar el pedido. Usa un navegador actualizado.'); return }
    }
    if (!pending.current) return
    busy.current = true; setSaving(true); onBusy(true)
    try {
      const result = await submitPublicOrder(pending.current)
      if (alive.current) onSuccess(result.total)
    } catch (failure) {
      if (!alive.current) return
      const ambiguous = !(failure instanceof ApiError) || failure.status === 0 || failure.status === 200 || failure.status === 409 || failure.status >= 500
      setUncertain(ambiguous)
      if (!ambiguous) pending.current = null
      setError(ambiguous ? 'No se pudo confirmar el resultado. Conservamos tu solicitud: puedes reenviarla con la misma clave sin crear otro pedido.' : failure instanceof Error ? failure.message : 'No se pudo enviar el pedido.')
    } finally { busy.current = false; if (alive.current) setSaving(false); onBusy(false) }
  }
  return <form className="public-checkout" onSubmit={submit}>
    <h3>Datos para la entrega</h3>
    <p>No necesitas crear una cuenta. El pedido quedará pendiente de aprobación.</p>
    {loading && <p role="status">Cargando zonas de entrega…</p>}
    {zoneError && <div role="alert"><p>{zoneError}</p><button type="button" onClick={() => setRevision(v => v + 1)}>Reintentar zonas</button></div>}
    {!loading && !zoneError && zones.length === 0 && <p role="status">No hay zonas de entrega disponibles por el momento.</p>}
    <fieldset disabled={saving || uncertain}>
      <label>Nombre completo<input ref={nameRef} value={name} onChange={e => setName(e.target.value)} required maxLength={150} autoComplete="name" /></label>
      <label>Teléfono<span className="public-checkout-phone"><span>+502</span><input aria-label="Teléfono: 8 dígitos de Guatemala" type="tel" inputMode="numeric" autoComplete="tel-national"
        value={phone} onChange={e => setPhone(e.target.value.replace(/[^0-9]/g, '').slice(0, 8))} required pattern="[0-9]{8}" maxLength={8} placeholder="12345678" /></span></label>
      <label>Zona de entrega<select value={zoneId} onChange={e => setZoneId(e.target.value)} required disabled={loading || !!zoneError}>
        <option value="">Selecciona tu zona</option>{zones.map(z => <option key={z.idZonaEntrega} value={z.idZonaEntrega}>{z.nombre}</option>)}
      </select></label>
      <label>Dirección exacta<textarea value={address} onChange={e => setAddress(e.target.value)} required maxLength={300} autoComplete="street-address" /></label>
      <label>Referencias (opcional)<input value={references} onChange={e => setReferences(e.target.value)} maxLength={300} /></label>
      <label>Observación (opcional)<textarea value={note} onChange={e => setNote(e.target.value)} maxLength={500} /></label>
    </fieldset>
    <p className="public-cart-total">Total estimado <strong>{money.format(total)}</strong></p>
    {zone && <p role="status">Para {zone.nombre}, el total debe ser mayor a {money.format(zone.minimoCompra)}.{!enough && ' Agrega más productos para continuar.'}</p>}
    <p>La disponibilidad y el total definitivo se verifican al enviar. El pedido no reserva inventario.</p>
    {items.length > 100 && <p role="alert">El pedido admite hasta 100 presentaciones diferentes.</p>}
    {error && <p role="alert">{error}</p>}
    <div className="public-checkout-actions">
      <button type="button" disabled={saving || uncertain} onClick={onBack}>Volver al carrito</button>
      <button type="submit" className="public-checkout-confirm" disabled={saving || (!uncertain && (loading || !!zoneError || !enough || items.length > 100))}>
        {saving ? 'Enviando pedido…' : uncertain ? 'Reintentar el mismo pedido' : 'Enviar pedido'}
      </button>
    </div>
  </form>
}
