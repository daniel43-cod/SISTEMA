import { useEffect, useId, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { receiptOptions, receiptStatus, sendReceipt } from '../api/comprobantesApi'
import type { ReceiptOptions, ReceiptResult } from '../api/comprobantesApi'
import './WhatsAppReceipt.css'

export function WhatsAppIcon() {
  return <svg width="21" height="21" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true"><path d="M13.6 2.3A7.8 7.8 0 0 0 1.3 11.7L.2 15.8l4.2-1.1A7.8 7.8 0 0 0 16 7.9a7.7 7.7 0 0 0-2.4-5.6ZM8.1 14.4a6.5 6.5 0 0 1-3.3-.9l-.2-.1-2.5.6.7-2.4-.2-.3a6.5 6.5 0 1 1 5.5 3.1Zm3.6-4.8c-.2-.1-1.2-.6-1.4-.7-.2 0-.3-.1-.4.1l-.6.7c-.1.1-.2.1-.4 0-.2-.1-.8-.3-1.5-.9-.5-.5-.9-1.1-1-1.3-.1-.2 0-.3.1-.4l.3-.3.2-.3c.1-.1 0-.3 0-.4l-.6-1.4c-.1-.4-.3-.3-.4-.3h-.4c-.2 0-.4.1-.5.2-.2.2-.7.7-.7 1.6s.7 1.9.8 2c.1.1 1.4 2.2 3.4 3 .4.2.8.3 1.1.4.5.1.9.1 1.2.1.4-.1 1.2-.5 1.4-1 .2-.5.2-.9.1-1-.1 0-.2-.1-.4-.2Z" /></svg>
}

export function WhatsAppReceiptDialog({ saleId, mode, onClose }: { saleId: number; mode: 'registro' | 'historial'; onClose: () => void }) {
  const { session, logout } = useAuth()
  const token = session?.token ?? ''
  const dialog = useRef<HTMLDialogElement>(null)
  const title = useId()
  const phoneInput = useRef<HTMLInputElement>(null)
  const alive = useRef(true)
  const busy = useRef(false)
  const sendController = useRef<AbortController | null>(null)
  const [options, setOptions] = useState<ReceiptOptions | null>(null)
  const [company, setCompany] = useState('')
  const [phone, setPhone] = useState('')
  const [loading, setLoading] = useState(true)
  const [sending, setSending] = useState(false)
  const [error, setError] = useState('')
  const [revision, setRevision] = useState(0)
  const [response, setResponse] = useState<ReceiptResult | null>(null)
  useEffect(() => {
    alive.current = true
    const previous = document.activeElement
    const panel = dialog.current
    panel?.showModal()
    return () => { alive.current = false; sendController.current?.abort(); panel?.close(); if (previous instanceof HTMLElement && previous.isConnected) previous.focus() }
  }, [])
  useEffect(() => {
    const controller = new AbortController()
    setLoading(true); setOptions(null); setError('')
    void receiptOptions(saleId, token, controller.signal).then(v => {
      if (!controller.signal.aborted) { setOptions(v); setCompany(v.empresas.length === 1 ? String(v.empresas[0].id) : '') }
    }).catch(e => {
      if (controller.signal.aborted) return
      if (e instanceof ApiError && e.status === 401) logout('La sesión ya no es válida.')
      else setError(e instanceof Error ? e.message : 'No se pudo preparar el comprobante.')
    }).finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [saleId, token, logout, revision])
  const sendId = response?.idEnvio
  useEffect(() => {
    if (!sendId || response?.huboFallo) return
    const controller = new AbortController()
    let timer: ReturnType<typeof setTimeout>
    let attempts = 0
    const poll = async () => {
      try {
        const latest = await receiptStatus(sendId, token, controller.signal)
        if (controller.signal.aborted) return
        setResponse(latest)
        if (latest.huboFallo || ['entregado', 'leido'].includes(latest.resultado.estado)) return
      } catch { if (controller.signal.aborted) return }
      if (++attempts < 20) timer = setTimeout(() => { void poll() }, 3000)
    }
    timer = setTimeout(() => { void poll() }, 3000)
    return () => { controller.abort(); clearTimeout(timer) }
    // El identificador cambia solo por una nueva acción de envío, no por cada actualización de estado.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [sendId, token])
  async function send() {
    if (busy.current || !options) return
    if (!options.telefono?.trim() && !phone.trim()) { setError('Ingresa el número de teléfono primero.'); phoneInput.current?.focus(); return }
    setError(''); setResponse(null)
    busy.current = true; setSending(true)
    const controller = new AbortController(); sendController.current = controller
    try {
      const result = await sendReceipt(saleId, Number(company), options.telefono, phone, mode, token, controller.signal)
      if (alive.current) {
        setResponse(result)
        if (mode === 'registro' && !options.telefono?.trim()) setOptions({ ...options, telefono: phone.trim() })
      }
    } catch (e) {
      if (!alive.current) return
      if (e instanceof ApiError && e.status === 401) logout('La sesión ya no es válida.')
      else setError(e instanceof Error ? e.message : 'Hubo un fallo al compartir el comprobante. Intenta nuevamente más tarde.')
      // Si se guardó el teléfono pero se perdió la respuesta, recargarlo evita intentar sustituirlo.
      void receiptOptions(saleId, token).then(v => { if (alive.current) setOptions(v) }).catch(() => {})
    } finally { busy.current = false; if (alive.current) setSending(false) }
  }
  return createPortal(<dialog ref={dialog} className="wa-receipt-dialog" aria-labelledby={title}
    onCancel={e => { e.preventDefault(); if (!busy.current) onClose() }}>
    <h2 id={title}>{mode === 'registro' ? `Venta #${saleId} registrada` : `Comprobante de venta #${saleId}`}</h2>
    <p>¿Quieres enviar el comprobante por WhatsApp?</p>
    {loading ? <p role="status">Cargando datos…</p> : options && <>
      {options.empresas.length > 1 && <label>Empresa del comprobante<select value={company} disabled={sending} onChange={e => setCompany(e.target.value)}>
        <option value="">Selecciona la empresa</option>{options.empresas.map(v => <option key={v.id} value={v.id}>{v.nombre}</option>)}
      </select></label>}
      {!options.empresas.length && <p role="alert">No hay una empresa configurada para el comprobante.</p>}
      {options.telefono?.trim() ? <p className="wa-receipt-destination">Enviar a <strong>{options.telefono}</strong></p> : <label>
        Número de teléfono<input ref={phoneInput} type="tel" inputMode="tel" autoComplete="off" maxLength={30} value={phone} disabled={sending}
          placeholder="Ej. 5555 1234" onChange={e => setPhone(e.target.value)} />
        <small>{mode === 'registro' ? 'Se guardará en los datos del cliente.' : 'Solo para este envío. No se guardará en la venta ni en el cliente.'}</small>
      </label>}
    </>}
    {error && <p className="wa-receipt-error" role="alert">{error}</p>}
    {!loading && !options && <button type="button" onClick={() => setRevision(v => v + 1)}>Reintentar carga</button>}
    {response && <p className={response.huboFallo ? 'wa-receipt-error' : 'wa-receipt-status'} role={response.huboFallo ? 'alert' : 'status'}>{response.mensaje}</p>}
    <div className="wa-receipt-actions">
      <button type="button" disabled={sending} onClick={onClose}>{response ? 'Cerrar' : 'Ahora no'}</button>
      <button type="button" className="wa-send-button" disabled={loading || sending || !options || !options.empresas.length} onClick={() => { void send() }}>
        <WhatsAppIcon />{sending ? 'Enviando…' : 'Enviar comprobante'}
      </button>
    </div>
  </dialog>, document.body)
}
