import { useEffect, useId, useRef, useState } from 'react'
import { ApiError } from '../../../shared/api/ApiError'
import { certifySale, loadFel } from '../api/felApi'
import type { FelDocument } from '../api/felApi'

export function CertifySalePanel({ saleId, token, active, onBusy, onUnauthorized }: {
  saleId: number; token: string; active: boolean; onBusy: (busy: boolean) => void; onUnauthorized: () => void
}) {
  const [document, setDocument] = useState<FelDocument | null>(null)
  const [loading, setLoading] = useState(true)
  const [sending, setSending] = useState(false)
  const [error, setError] = useState('')
  const [revision, setRevision] = useState(0)
  const [address, setAddress] = useState('')
  const [postal, setPostal] = useState('')
  const [municipality, setMunicipality] = useState('')
  const [department, setDepartment] = useState('')
  const [country, setCountry] = useState('GT')
  const [confirmed, setConfirmed] = useState(false)
  const busy = useRef(false)
  const alive = useRef(true)
  const titleId = useId()
  useEffect(() => { alive.current = true; return () => { alive.current = false } }, [])
  useEffect(() => {
    const controller = new AbortController()
    setLoading(true)
    void loadFel(saleId, token, controller.signal).then(value => {
      if (!controller.signal.aborted) { setDocument(value); setError('') }
    }).catch(e => {
      if (controller.signal.aborted) return
      if (e instanceof ApiError && e.status === 401) onUnauthorized()
      else setError(e instanceof Error ? e.message : 'No se pudo consultar la certificación.')
    }).finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [saleId, token, revision, onUnauthorized])
  async function send(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (busy.current || loading || document?.estado === 'CERTIFICADA' || document?.estado === 'EN_PROCESO') return
    busy.current = true; setSending(true); onBusy(true); setError('')
    try {
      const result = await certifySale(saleId, {
        direccion: address.trim(), codigo_postal: postal, municipio: municipality.trim(),
        departamento: department.trim(), pais: country.toUpperCase(), confirmar_bienes_gravados: confirmed,
      }, token)
      if (alive.current) setDocument(result)
    } catch (e) {
      if (!alive.current) return
      if (e instanceof ApiError && e.status === 401) onUnauthorized()
      else setError((e instanceof Error ? e.message : 'No se pudo confirmar el envío.') + ' Consulta el estado antes de volver a enviar.')
    } finally { busy.current = false; if (alive.current) { setSending(false); onBusy(false) } }
  }
  if (!active) return null
  return <section className="fel-certification-panel" aria-labelledby={titleId}>
    <h3 id={titleId}>Certificar factura · Venta #{saleId}</h3>
    {loading && <p role="status">Consultando estado de la factura…</p>}
    {document && <p role="status">{document.estado === 'CERTIFICADA' ? 'Factura certificada' : `Estado: ${document.estado.replaceAll('_', ' ').toLowerCase()}`} · {document.ambiente}</p>}
    {document?.ambiente === 'PRUEBAS' && <p>Documento de pruebas, sin validez fiscal.</p>}
    {document?.descripcion && <p>{document.descripcion}</p>}
    {document?.errores.map((e, i) => <p key={i} className="wa-receipt-error" role="alert">{e.mensaje}</p>)}
    {document?.alertas.map((a, i) => <p key={i}>{a.mensaje}</p>)}
    {error && <p className="wa-receipt-error" role="alert">{error}</p>}
    {document?.estado === 'CERTIFICADA' && <>
      <p>Serie: {document.serie} · Número: {document.numero}</p>
      {document.pdf && <a className="fel-pdf-link" href={document.pdf} target="_blank" rel="noopener noreferrer">Ver factura PDF</a>}
    </>}
    {!loading && document?.estado !== 'CERTIFICADA' && document?.estado !== 'EN_PROCESO' && <form onSubmit={event => { void send(event) }}>
      <p>Confirma la dirección fiscal del receptor. Su identificación se toma de la venta.</p>
      <fieldset disabled={sending}>
        <label>Dirección<input required maxLength={250} value={address} onChange={e => setAddress(e.target.value)} /></label>
        <label>Código postal<input required inputMode="numeric" pattern="[0-9]+" maxLength={10} value={postal} onChange={e => setPostal(e.target.value.replace(/[^0-9]/g, ''))} /></label>
        <label>Municipio<input required maxLength={100} value={municipality} onChange={e => setMunicipality(e.target.value)} /></label>
        <label>Departamento<input required maxLength={100} value={department} onChange={e => setDepartment(e.target.value)} /></label>
        <label>País<input required pattern="[A-Za-z]{2}" maxLength={2} value={country} onChange={e => setCountry(e.target.value.toUpperCase().replace(/[^A-Z]/g, ''))} /></label>
        <label className="fel-confirmation"><input type="checkbox" required checked={confirmed} onChange={e => setConfirmed(e.target.checked)} />Los productos están gravados con IVA incluido del 12 %.</label>
        {document?.estado === 'POR_CONFIRMAR' && <p>El reintento conserva el XML anterior; los cambios en estos campos no se aplican.</p>}
        <button type="submit" disabled={sending}>{sending ? 'Certificando…' : document?.estado === 'RECHAZADA' ? 'Certificar con datos corregidos' : document?.estado === 'POR_CONFIRMAR' ? 'Confirmar envío anterior' : 'Confirmar certificación'}</button>
      </fieldset>
    </form>}
    <button type="button" disabled={loading || sending} onClick={() => setRevision(v => v + 1)}>Consultar estado</button>
  </section>
}
