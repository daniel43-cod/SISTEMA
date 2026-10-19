import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { createUser, updateUser, changeUserState, listUsers, listUserRoles } from '../api/usersApi'
import type { StaffUser, UserRole } from '../api/usersApi'
import './UsersPage.css'

export function UsersPage() {
  const { session, logout } = useAuth()
  const token = session?.token
  const allowed = session?.user.role === 'ADMINISTRADOR'
  const [items, setItems] = useState<StaffUser[]>([])
  const [roles, setRoles] = useState<UserRole[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [formError, setFormError] = useState('')
  const [success, setSuccess] = useState('')
  const [revision, setRevision] = useState(0)
  const [editing, setEditing] = useState<StaffUser | null>(null)
  const [formVersion, setFormVersion] = useState(0)
  const [open, setOpen] = useState(false)
  const [saving, setSaving] = useState(false)
  const [uncertain, setUncertain] = useState(false)
  const busy = useRef(false)
  const alive = useRef(true)
  const dialogRef = useRef<HTMLDialogElement>(null)
  const formRef = useRef<HTMLFormElement>(null)
  const createButton = useRef<HTMLButtonElement>(null)
  useEffect(() => { alive.current = true; return () => { alive.current = false } }, [])
  useEffect(() => {
    if (!token || !allowed) return
    const controller = new AbortController()
    async function load() {
      setLoading(true); setError('')
      try {
        const [users, userRoles] = await Promise.all([listUsers(token!, controller.signal), listUserRoles(token!, controller.signal)])
        if (!controller.signal.aborted) { setItems(users); setRoles(userRoles) }
      } catch (failure) {
        if (controller.signal.aborted) return
        if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
        else setError(failure instanceof Error ? failure.message : 'No se pudieron cargar los usuarios.')
      } finally { if (!controller.signal.aborted) setLoading(false) }
    }
    void load()
    return () => controller.abort()
  }, [token, allowed, revision, logout])
  useEffect(() => {
    if (!open || !dialogRef.current) return
    const dialog = dialogRef.current
    const trigger = document.activeElement instanceof HTMLElement ? document.activeElement : createButton.current
    dialog.showModal()
    const previous = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    return () => { dialog.close(); document.body.style.overflow = previous; trigger?.focus() }
  }, [open])
  if (!allowed) return null
  const availableRoles = roles.filter(role => role.estado && ['ADMINISTRADOR', 'VENDEDOR'].includes(role.nombre))
  async function toggleState(item: StaffUser) {
    if (!token || busy.current || item.id_usuario === session?.user.id) return
    if (!window.confirm(`${item.estado ? '¿Desactivar' : '¿Activar'} al usuario ${item.usuario}?`)) return
    busy.current = true; setSaving(true); setError(''); setSuccess('')
    try {
      const updated = await changeUserState(token, item.id_usuario, !item.estado)
      if (!alive.current) return
      setItems(rows => rows.map(row => row.id_usuario === updated.id_usuario ? updated : row))
      setSuccess(updated.estado ? 'Usuario activado.' : 'Usuario desactivado. Sus sesiones anteriores quedaron revocadas.')
    } catch (failure) {
      if (!alive.current) return
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else setError(failure instanceof Error ? failure.message : 'No se pudo confirmar el estado. Actualiza la lista.')
    } finally { busy.current = false; if (alive.current) setSaving(false) }
  }
  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!token || busy.current || uncertain) return
    const form = new FormData(event.currentTarget)
    const value = (key: string) => String(form.get(key) ?? '').trim()
    const password = String(form.get('password') ?? '')
    const idRole = Number(value('id_rol'))
    if (!availableRoles.some(role => role.id_rol === idRole)) { setFormError('Selecciona un rol activo.'); return }
    if (!editing && (password.length < 8 || new TextEncoder().encode(password).length > 72 ||
        !/[a-z]/.test(password) || !/[A-Z]/.test(password) || !/[0-9]/.test(password))) {
      setFormError('La contraseña debe tener al menos 8 caracteres, mayúscula, minúscula y número, y no superar 72 bytes.'); return
    }
    if (!/^[0-9+() -]+$/.test(value('telefono')) || !/[0-9]/.test(value('telefono'))) {
      setFormError('Ingresa un teléfono válido.'); return
    }
    if (['nombre', 'apellido', 'usuario'].some(key => !value(key))) { setFormError('Completa los campos obligatorios.'); return }
    busy.current = true; setSaving(true); setFormError(''); setSuccess('')
    try {
      const body = { nombre: value('nombre'), apellido: value('apellido'), usuario: value('usuario'),
        telefono: value('telefono'), corre_electronico: value('correo') || null, id_rol: idRole }
      if (editing) await updateUser(token, editing.id_usuario, body)
      else await createUser(token, { ...body, password })
      if (editing?.id_usuario === session?.user.id && editing.id_rol !== idRole) {
        logout('Tu rol cambió. Inicia sesión nuevamente.'); return
      }
      if (!alive.current) return
      formRef.current?.reset(); setOpen(false); setSuccess(editing ? 'Usuario actualizado correctamente.' : 'Usuario creado correctamente.'); setRevision(v => v + 1)
    } catch (failure) {
      if (!alive.current) return
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else {
        const ambiguous = !(failure instanceof ApiError) || failure.status === 0 || failure.status >= 500
        setUncertain(ambiguous)
        setFormError(ambiguous ? 'No se pudo confirmar la operación. Cierra el formulario y actualiza la lista para comprobar si los datos se guardaron antes de reintentar.'
          : failure.message)
      }
    } finally { busy.current = false; if (alive.current) setSaving(false) }
  }
  return <section className="users-panel" aria-labelledby="users-title">
    <div className="users-heading"><h2 id="users-title">Usuarios</h2>
      <button type="button" disabled={loading || saving} onClick={() => setRevision(v => v + 1)}>Actualizar</button></div>
    {success && <p role="status" className="users-success">{success}</p>}
    {loading ? <p role="status">Cargando usuarios…</p> : error ? <div role="alert"><p>{error}</p>
      <button type="button" onClick={() => setRevision(v => v + 1)}>Reintentar</button></div>
      : !items.length ? <p>No hay usuarios registrados.</p> : <div className="users-table-scroll" tabIndex={0} role="region" aria-label="Lista de usuarios">
        <table><thead><tr>{['Nombre', 'Apellido', 'Usuario', 'Correo', 'Teléfono', 'Estado', 'Rol', 'Acciones'].map(label => <th key={label} scope="col">{label}</th>)}</tr></thead>
          <tbody>{items.map(item => <tr key={item.id_usuario}>
            <td>{item.nombre}</td><td>{item.apellido}</td><td>{item.usuario}</td><td>{item.correo || '—'}</td><td>{item.telefono}</td>
            <td><span className={item.estado ? 'users-active' : 'users-inactive'}>{item.estado ? 'Activo' : 'Inactivo'}</span></td>
            <td>{roles.find(role => role.id_rol === item.id_rol)?.nombre ?? 'Rol #' + item.id_rol}</td>
            <td><button type="button" disabled={saving} aria-label={'Editar usuario ' + item.usuario}
              aria-haspopup="dialog" aria-controls="users-create-dialog" onClick={() => {
                setEditing(item); setFormVersion(v => v + 1); setFormError(''); setUncertain(false); setOpen(true)
              }}>Editar</button>
              <button type="button" disabled={saving || item.id_usuario === session?.user.id}
                title={item.id_usuario === session?.user.id ? 'No puedes desactivar tu propia cuenta' : undefined}
                aria-label={(item.estado ? 'Desactivar a ' : 'Activar a ') + item.usuario}
                onClick={() => { void toggleState(item) }}>{item.estado ? 'Desactivar' : 'Activar'}</button>
            </td>
          </tr>)}</tbody></table>
      </div>}
    <button ref={createButton} type="button" className="users-create-floating" title="Crear nuevo usuario" aria-label="Crear nuevo usuario"
      aria-haspopup="dialog" aria-controls="users-create-dialog" disabled={loading || saving || !!error || !availableRoles.length}
      onClick={() => { setEditing(null); setFormVersion(v => v + 1); setFormError(''); setUncertain(false); setOpen(true) }}>
      <svg width="26" height="26" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" aria-hidden="true"><path d="M12 5v14M5 12h14" /></svg>
    </button>
    {!loading && !error && !availableRoles.length && <p role="status">No hay roles internos activos para crear usuarios.</p>}
    <dialog ref={dialogRef} id="users-create-dialog" className="users-dialog" aria-labelledby="users-create-title"
      onCancel={event => { if (saving) event.preventDefault(); else setOpen(false) }} onClose={() => { formRef.current?.reset(); setOpen(false) }}>
      <div className="users-heading"><h2 id="users-create-title">{editing ? 'Editar usuario' : 'Crear usuario'}</h2><button type="button" disabled={saving} aria-label="Cerrar formulario" onClick={() => setOpen(false)}>×</button></div>
      <form key={formVersion} ref={formRef} onSubmit={submit} aria-busy={saving}>
        <fieldset disabled={saving || uncertain}><legend className="sr-only">Datos del usuario</legend>
          <label>Nombre<input name="nombre" defaultValue={editing?.nombre ?? ''} autoComplete="given-name" required maxLength={100} /></label>
          <label>Apellido<input name="apellido" defaultValue={editing?.apellido ?? ''} autoComplete="family-name" required maxLength={100} /></label>
          <label>Usuario<input name="usuario" defaultValue={editing?.usuario ?? ''} autoComplete="off" required maxLength={50} /></label>
          <label>Correo (opcional)<input name="correo" defaultValue={editing?.correo ?? ''} type="email" autoComplete="email" maxLength={254} /></label>
          <label>Teléfono<input name="telefono" defaultValue={editing?.telefono ?? ''} type="tel" autoComplete="tel" required maxLength={25} /></label>
          <label>Rol<select name="id_rol" required defaultValue={editing?.id_rol ?? ''}><option value="" disabled>Selecciona un rol</option>
            {editing && !availableRoles.some(role => role.id_rol === editing.id_rol) && <option value={editing.id_rol} disabled>Rol actual inactivo: selecciona otro</option>}
            {availableRoles.map(role => <option key={role.id_rol} value={role.id_rol}>{role.nombre}</option>)}</select></label>
          {!editing && <><label>Contraseña<input name="password" type="password" autoComplete="new-password" required minLength={8} maxLength={72} aria-describedby="users-password-help" /></label>
          <p id="users-password-help">Mínimo 8 caracteres, con mayúscula, minúscula y número. Máximo 72 bytes.</p></>}
          <button className="users-submit" type="submit">{saving ? 'Guardando…' : editing ? 'Guardar cambios' : 'Crear usuario'}</button>
        </fieldset>
      </form>
      {formError && <p role="alert" className="users-error">{formError}</p>}
    </dialog>
  </section>
}
