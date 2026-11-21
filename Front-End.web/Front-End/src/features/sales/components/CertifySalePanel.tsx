import { useEffect, useId, useRef, useState } from 'react'
import { ApiError } from '../../../shared/api/ApiError'
import { certifySale, loadFel, loadFelPreparation, recoverFel } from '../api/felApi'
import type { FelDocument } from '../api/felApi'
import { lookupReceiver } from '../api/receptorLookupApi'
import { receiverDocumentInput } from '../utils/saleReceiver'
import type { ReceiverType } from '../utils/saleReceiver'

export function CertifySalePanel({ saleId, token, active, onBusy, onUnauthorized, onToggle, disabled = false }: {
  saleId: number; token: string; active: boolean; onToggle: () => void; disabled?: boolean; onBusy: (busy: boolean) => void; onUnauthorized: () => void
}) {
  const [document, setDocument] = useState<FelDocument | null>(null)
  const [total, setTotal] = useState<number | null>(null)
  const [type, setType] = useState<ReceiverType>('')
  const [identification, setIdentification] = useState('')
  const [verified, setVerified] = useState<{ key: string; name: string } | null>(null)
  const [lookupPending, setLookupPending] = useState(false)
  const [loading, setLoading] = useState(true)
  const [sending, setSending] = useState(false)
  const [error, setError] = useState('')
  const [revision, setRevision] = useState(0)
  const [address, setAddress] = useState('')
  const [postal, setPostal] = useState('')
  const [municipality, setMunicipality] = useState('')
  const [department, setDepartment] = useState('')
  const [country, setCountry] = useState('GT')
  const busy = useRef(false)
  const alive = useRef(true)
  const lookupController = useRef<AbortController | null>(null)
  const currentKey = useRef('')
  currentKey.current = type + ':' + identification
  const titleId = useId()
  const reconciliation = document?.estado === 'POR_CONFIRMAR'
  const hasAddress = [address, postal, municipality, department].some(value => value.trim().length > 0)
  useEffect(() => { alive.current = true; return () => { alive.current = false; lookupController.current?.abort() } }, [])
  useEffect(() => {
    const controller = new AbortController()
    setLoading(true); setTotal(null)
    void Promise.all([loadFel(saleId, token, controller.signal), loadFelPreparation(saleId, token, controller.signal)]).then(([invoice, sale]) => {
      if (controller.signal.aborted) return
      setDocument(invoice); setTotal(sale.total); setError('')
    }).catch(e => {
      if (controller.signal.aborted) return
      if (e instanceof ApiError && e.status === 401) onUnauthorized()
      else setError(e instanceof Error ? e.message : 'No se pudo preparar la certificación.')
    }).finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [saleId, token, revision, onUnauthorized])
  function invalidate() {
    lookupController.current?.abort(); setVerified(null); setLookupPending(false); setError('')
  }
  async function lookup() {
    if (type !== 'NIT' && type !== 'CUI') return
    if (type === 'CUI' ? !/^[0-9]{13}$/.test(identification) : !/^[0-9]+[0-9K]$/.test(identification)) {
      setError(type === 'CUI' ? 'Ingresa los 13 números del CUI.' : 'Ingresa un NIT válido.'); return
    }
    invalidate()
    const controller = new AbortController(); lookupController.current = controller
    const key = currentKey.current
    setLookupPending(true)
    try {
      const name = await lookupReceiver(type, identification, token, controller.signal)
      if (!controller.signal.aborted && currentKey.current === key) setVerified({ key, name })
    } catch (e) {
      if (!controller.signal.aborted && alive.current) {
        if (e instanceof ApiError && e.status === 401) onUnauthorized()
        else setError(e instanceof Error ? e.message : 'No se pudo consultar el documento.')
      }
    } finally { if (!controller.signal.aborted && alive.current) setLookupPending(false) }
  }
  async function send(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (busy.current || disabled || loading || total === null || document?.estado === 'CERTIFICADA' || document?.estado === 'EN_PROCESO') return
    if (!reconciliation) {
      if (!type) { setError('Elegí una opción: consumidor final, NIT o CUI.'); return }
      if (type === 'CF' && total >= 2500) { setError('Desde Q2,500.00 debes seleccionar NIT o CUI.'); return }
      if (type !== 'CF' && (lookupPending || verified?.key !== currentKey.current)) { setError('Consulta el documento y verifica el nombre antes de certificar.'); return }
    }
    busy.current = true; setSending(true); onBusy(true); setError('')
    try {
      const result = reconciliation ? await recoverFel(saleId, token) : await certifySale(saleId, {
        ...(reconciliation ? {} : { tipo_identificacion_receptor: type || undefined, identificacion_receptor: type === 'CF' ? 'CF' : identification }),
        ...(!reconciliation && type !== 'CF' && hasAddress ? { direccion: address.trim(), codigo_postal: postal, municipio: municipality.trim(), departamento: department.trim(), pais: country } : {}),
        confirmar_bienes_gravados: true,
      }, token)
      if (alive.current) setDocument(result)
    } catch (e) {
      if (!alive.current) return
      if (e instanceof ApiError && e.status === 401) onUnauthorized()
      else setError((e instanceof Error ? e.message : 'No se pudo confirmar el envío.') + ' Consulta el estado antes de volver a enviar.')
    } finally { busy.current = false; if (alive.current) { setSending(false); onBusy(false) } }
  }
  async function recover() {
    if (busy.current || disabled || !document?.puede_recuperar) return
    busy.current = true; setSending(true); onBusy(true); setError('')
    try {
      const result = await recoverFel(saleId, token)
      if (alive.current) setDocument(result)
    } catch (e) {
      if (!alive.current) return
      if (e instanceof ApiError && e.status === 401) onUnauthorized()
      else setError('No se pudo completar la recuperación. Consulta el estado; no registres otra factura.')
    } finally { busy.current = false; if (alive.current) { setSending(false); onBusy(false) } }
  }
  return <section className="fel-certification-panel" aria-labelledby={titleId}>
    <header className="fel-card-header">
      <span className="fel-card-icon" aria-hidden="true"><svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8"><path d="M6 3h12v18l-3-2-3 2-3-2-3 2V3Z"/><path d="M9 7h6M9 11h6M9 15h3"/></svg></span>
      <div className="fel-card-heading"><h3 id={titleId}>Certificar factura</h3><p>Venta #{saleId}</p></div>
      <span className={document?.estado === 'CERTIFICADA' ? 'fel-badge fel-badge-success' : document?.estado === 'RECHAZADA' ? 'fel-badge fel-badge-error' : 'fel-badge'}>
        {document?.estado === 'CERTIFICADA' ? 'Certificada' : document?.estado === 'RECHAZADA' ? 'Rechazada' : document?.estado === 'EN_PROCESO' ? 'En proceso' : document ? 'Por confirmar' : 'Sin certificar'}
      </span>
      <button className="fel-toggle" type="button" disabled={disabled || sending} aria-expanded={active} aria-controls={titleId + '-body'} onClick={onToggle}>{active ? 'Ocultar' : 'Mostrar'}</button>
    </header>
    <div id={titleId + '-body'} className="fel-card-body" hidden={!active}>
    {loading && <p role="status">Consultando datos de la venta…</p>}
    {document && <p role="status">{document.estado === 'CERTIFICADA' ? 'Factura certificada' : `Estado: ${document.estado.replaceAll('_', ' ').toLowerCase()}`} · {document.ambiente}</p>}
    {document?.ambiente === 'PRUEBAS' && <p>Documento de pruebas, sin validez fiscal.</p>}
    {document?.descripcion && <p>{document.descripcion}</p>}
    {document?.estado === 'EN_PROCESO' && <div className="fel-recovery-notice" role="status">
      {document.puede_recuperar ? <>
        <p>El plazo de procesamiento venció. Puedes recuperar el resultado sin crear otra factura.</p>
        <button className="fel-primary-button" type="button" disabled={sending || disabled} onClick={() => { void recover() }}>{sending ? 'Recuperando…' : 'Recuperar certificación'}</button>
      </> : <p>La certificación sigue en proceso. {document.procesamiento_vence && <>Puedes volver a consultar después de {new Date(document.procesamiento_vence).toLocaleTimeString('es-GT', { timeZone: 'America/Guatemala' })}.</>}</p>}
    </div>}
    {document?.errores.map((e, i) => <p key={i} className="wa-receipt-error" role="alert">{e.mensaje}</p>)}
    {document?.alertas.map((a, i) => <p key={i}>{a.mensaje}</p>)}
    {error && <p className="wa-receipt-error" role="alert">{error}</p>}
    {document?.estado === 'CERTIFICADA' && <><button className="fel-primary-button" type="button" disabled>Factura ya certificada</button><p>Serie: {document.serie} · Número: {document.numero}</p>
      {document.pdf && <a className="fel-pdf-link" href={document.pdf} target="_blank" rel="noopener noreferrer">Ver factura PDF</a>}</>}
    {!loading && total !== null && document?.estado !== 'CERTIFICADA' && document?.estado !== 'EN_PROCESO' && <form onSubmit={e => { void send(e) }}>
      <fieldset disabled={sending || disabled}>
        {reconciliation ? <p>Se confirmará el envío anterior con el mismo XML e identificador.</p> : <>
          <label>Tipo de identificación<select required value={type} onChange={e => { invalidate(); setType(e.target.value as ReceiverType); setIdentification('') }}>
            <option value="" disabled>Elegí una opción</option><option value="CF" disabled={total >= 2500}>Consumidor final</option><option value="NIT">NIT</option><option value="CUI">CUI</option>
          </select></label>
          {total >= 2500 && <p>Desde Q2,500.00 no se permite consumidor final; selecciona NIT o CUI.</p>}
          {type === 'CF' ? <p className="fel-info"><svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true"><circle cx="12" cy="12" r="9"/><path d="M12 11v6M12 7v1"/></svg><span>Se certificará a nombre de CONSUMIDOR FINAL, sin dirección del receptor.</span></p> : type ? <>
            <label>{type}<input required inputMode={type === 'CUI' ? 'numeric' : 'text'} value={identification}
              minLength={type === 'CUI' ? 13 : 2} maxLength={type === 'CUI' ? 13 : 20}
              pattern={type === 'CUI' ? '[0-9]{13}' : '[0-9]+[0-9K]'}
              onChange={e => { invalidate(); setIdentification(receiverDocumentInput(type, e.target.value)) }}
              onKeyDown={e => { if (e.key === 'Enter') { e.preventDefault(); void lookup() } }} /></label>
            <button type="button" disabled={lookupPending || !identification} onClick={() => { void lookup() }}>{lookupPending ? 'Consultando…' : 'Consultar'}</button>
            {verified?.key === currentKey.current && <label>Nombre registrado<input value={verified!.name} readOnly /></label>}
            <details className="fel-address"><summary>Agregar dirección fiscal (opcional)</summary><p>Si ingresas una dirección, completa todos sus campos.</p>
            <label>Dirección<input required={hasAddress} maxLength={250} value={address} onChange={e => setAddress(e.target.value)} /></label>
            <label>Código postal<input required={hasAddress} inputMode="numeric" pattern="[0-9]+" maxLength={10} value={postal} onChange={e => setPostal(e.target.value.replace(/[^0-9]/g, ''))} /></label>
            <label>Municipio<input required={hasAddress} maxLength={100} value={municipality} onChange={e => setMunicipality(e.target.value)} /></label>
            <label>Departamento<input required={hasAddress} maxLength={100} value={department} onChange={e => setDepartment(e.target.value)} /></label>
            <label>País<input required={hasAddress} pattern="[A-Z]{2}" maxLength={2} value={country} onChange={e => setCountry(e.target.value.toUpperCase().replace(/[^A-Z]/g, ''))} /></label>
            </details>
          </> : null}
        </>}
        <div className="fel-card-actions">
        <button className="fel-primary-button" type="submit" disabled={sending || (!reconciliation && (!type || (type === 'CF' ? total >= 2500 : lookupPending || verified?.key !== currentKey.current)))}>
          {sending ? 'Certificando…' : reconciliation ? 'Recuperar certificación' : 'Certificar factura'}
        </button>
        <button className="fel-secondary-button" type="button" disabled={loading || sending || disabled} onClick={() => setRevision(v => v + 1)}>Consultar estado</button>
        </div>
      </fieldset>
    </form>}
    {(document?.estado === 'CERTIFICADA' || document?.estado === 'EN_PROCESO' || total === null) && <button className="fel-secondary-button" type="button" disabled={loading || sending || disabled} onClick={() => setRevision(v => v + 1)}>Consultar estado</button>}
    </div>
  </section>
}
