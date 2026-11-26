import { useEffect, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import { ApiError } from '../../../shared/api/ApiError'
import { updateClient, clientPhoneInput, validClientPhone, clientDocumentInput, validClientDocument } from '../api/clientsApi'
import type { ClientEdit, ClientSummary } from '../api/clientsApi'

export function EditClientDialog({ summary, token, onSaved, onClose, fail }: { summary: ClientSummary; token: string; onSaved: (updated: ClientSummary) => void; onClose: () => void; fail: (e: unknown) => string }) {
  const c = summary.cliente
  const [form, setForm] = useState<ClientEdit>(() => ({ nombre: c.nombre, apellido: c.apellido ?? '', nit: c.nit ?? '', dpi: c.dpi ?? '', telefono: c.telefono ?? '', correoElectronico: c.correo_electronico ?? '', direccion: c.direccion ?? '' }))
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const busy = useRef(false)
  const alive = useRef(true)
  const dialog = useRef<HTMLDialogElement>(null)
  const name = useRef<HTMLInputElement>(null)
  const phone = useRef<HTMLInputElement>(null)
  const dpi = useRef<HTMLInputElement>(null)
  const nit = useRef<HTMLInputElement>(null)
  useEffect(() => {
    alive.current = true
    const previous = document.activeElement
    const overflow = document.body.style.overflow
    const panel = dialog.current
    panel?.showModal(); document.body.style.overflow = 'hidden'; name.current?.focus()
    return () => { alive.current = false; panel?.close(); document.body.style.overflow = overflow; if (previous instanceof HTMLElement && previous.isConnected) previous.focus() }
  }, [])
  async function submit(event: React.FormEvent) {
    event.preventDefault()
    if (busy.current) return
    if (!form.nombre.trim()) { setError('Ingresa el nombre del cliente.'); name.current?.focus(); return }
    if (!validClientPhone(form.telefono)) { setError('El teléfono debe tener exactamente 8 dígitos o quedar vacío.'); phone.current?.focus(); return }
    if (!validClientDocument(form.dpi)) { setError('El DPI debe contener únicamente números o quedar vacío.'); dpi.current?.focus(); return }
    if (!validClientDocument(form.nit)) { setError('El NIT debe contener únicamente números o quedar vacío.'); nit.current?.focus(); return }
    busy.current = true; setSaving(true); setError('')
    try {
      const updated = await updateClient(token, c.id_cliente, form)
      if (alive.current) onSaved(updated)
    } catch (e) {
      if (alive.current) setError(e instanceof ApiError && (e.status === 0 || e.status >= 500)
        ? 'No se pudo confirmar el cambio. Conservamos los datos; podés volver a guardar.' : fail(e))
    } finally { busy.current = false; if (alive.current) setSaving(false) }
  }
  const fields: { key: keyof ClientEdit; label: string; max: number; type?: string }[] = [
    { key: 'nombre', label: 'Nombre', max: 100 }, { key: 'apellido', label: 'Apellido', max: 100 },
    { key: 'nit', label: 'NIT', max: 20 }, { key: 'dpi', label: 'DPI', max: 20 },
    { key: 'telefono', label: 'Teléfono', max: 8, type: 'tel' }, { key: 'correoElectronico', label: 'Correo electrónico', max: 254, type: 'email' },
    { key: 'direccion', label: 'Dirección', max: 200 },
  ]
  return createPortal(<dialog ref={dialog} className="clients-edit-dialog" aria-labelledby="clients-edit-title" onCancel={event => { event.preventDefault(); if (!busy.current) onClose() }}>
    <form onSubmit={submit}>
      <header><h2 id="clients-edit-title">Editar cliente</h2><div><button type="submit" disabled={saving}>{saving ? 'Guardando…' : 'Guardar'}</button><button type="button" aria-label="Cerrar edición de cliente" disabled={saving} onClick={onClose}>×</button></div></header>
      <fieldset disabled={saving}>{fields.map(field => <label key={field.key}>{field.label}{field.key === 'nombre' ? ' *' : ''}
        <input ref={field.key === 'nombre' ? name : field.key === 'telefono' ? phone : field.key === 'dpi' ? dpi : field.key === 'nit' ? nit : undefined} type={field.type ?? 'text'} value={form[field.key]} required={field.key === 'nombre'} maxLength={field.max}
          inputMode={['telefono', 'dpi', 'nit'].includes(field.key) ? 'numeric' : undefined} pattern={field.key === 'telefono' ? '[0-9]{8}' : ['dpi', 'nit'].includes(field.key) ? '[0-9]+' : undefined}
          title={field.key === 'telefono' ? '8 dígitos o vacío' : undefined}
          onInvalid={e => { if (field.key === 'telefono') e.currentTarget.setCustomValidity('El teléfono debe tener exactamente 8 dígitos o quedar vacío.') }}
          onChange={e => { e.currentTarget.setCustomValidity(''); const value = field.key === 'telefono' ? clientPhoneInput(e.target.value) : ['dpi', 'nit'].includes(field.key) ? clientDocumentInput(e.target.value) : e.target.value; setForm(f => ({ ...f, [field.key]: value })) }} /></label>)}</fieldset>
      {error && <p role="alert">{error}</p>}
    </form>
  </dialog>, document.body)
}
