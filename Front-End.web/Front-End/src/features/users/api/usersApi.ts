import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'

export interface StaffUser {
  id_usuario: number; id_rol: number; nombre: string; apellido: string; usuario: string
  correo: string | null; telefono: string; estado: boolean
}
export interface UserRole { id_rol: number; nombre: string; estado: boolean }
export interface NewUser {
  nombre: string; apellido: string; usuario: string; corre_electronico: string | null
  telefono: string; password: string; id_rol: number
}
function parseUser(value: unknown): StaffUser {
  const row = value as Partial<StaffUser> | null
  if (!row || !Number.isSafeInteger(row.id_usuario) || row.id_usuario! <= 0 ||
      !Number.isSafeInteger(row.id_rol) || row.id_rol! <= 0 ||
      !['nombre', 'apellido', 'usuario', 'telefono'].every(key => typeof (row as Record<string, unknown>)[key] === 'string') ||
      (row.correo !== null && typeof row.correo !== 'string') || typeof row.estado !== 'boolean')
    throw new ApiError('El usuario recibido no es válido.')
  return { id_usuario: row.id_usuario!, id_rol: row.id_rol!, nombre: row.nombre!, apellido: row.apellido!,
    usuario: row.usuario!, correo: row.correo, telefono: row.telefono!, estado: row.estado }
}
export async function listUsers(token: string, signal?: AbortSignal): Promise<StaffUser[]> {
  const result = await requestJson('/Usuario', { token, signal })
  if (!Array.isArray(result)) throw new ApiError('El listado de usuarios no es válido.')
  const rows = result.map(parseUser)
  if (new Set(rows.map(row => row.id_usuario)).size !== rows.length) throw new ApiError('El listado contiene usuarios duplicados.')
  return rows
}
export async function listUserRoles(token: string, signal?: AbortSignal): Promise<UserRole[]> {
  const result = await requestJson('/Rol', { token, signal })
  if (!Array.isArray(result)) throw new ApiError('El listado de roles no es válido.')
  const rows = result.map(value => {
    const row = value as Partial<UserRole> | null
    if (!row || !Number.isSafeInteger(row.id_rol) || row.id_rol! <= 0 || typeof row.nombre !== 'string' ||
        !row.nombre.trim() || typeof row.estado !== 'boolean') throw new ApiError('El rol recibido no es válido.')
    return { id_rol: row.id_rol!, nombre: row.nombre, estado: row.estado }
  })
  if (new Set(rows.map(row => row.id_rol)).size !== rows.length) throw new ApiError('El listado contiene roles duplicados.')
  return rows
}
export async function createUser(token: string, body: NewUser): Promise<StaffUser> {
  try { return parseUser(await requestJson('/Usuario', { method: 'POST', token, body, validationMessages: true })) }
  catch (failure) {
    if (failure instanceof ApiError && failure.status === 409)
      throw new ApiError('Ya existe un usuario con ese nombre de usuario, correo o teléfono.', 409)
    throw failure
  }
}

export async function updateUser(token: string, id: number, body: Omit<NewUser, 'password'>): Promise<StaffUser> {
  if (!Number.isSafeInteger(id) || id <= 0) throw new ApiError('Usuario inválido.', 400)
  try {
    const result = parseUser(await requestJson(`/Usuario/${id}`, { method: 'PUT', token, body, validationMessages: true }))
    if (result.id_usuario !== id) throw new ApiError('No se pudo confirmar la edición del usuario.')
    return result
  } catch (failure) {
    if (failure instanceof ApiError && failure.status === 409)
      throw new ApiError('Otro usuario ya utiliza ese nombre de usuario, correo o teléfono.', 409)
    if (failure instanceof ApiError && failure.status === 404)
      throw new ApiError('El usuario ya no existe. Actualiza la lista.', 404)
    throw failure
  }
}
export async function changeUserState(token: string, id: number, estado: boolean): Promise<StaffUser> {
  if (!Number.isSafeInteger(id) || id <= 0) throw new ApiError('Usuario inválido.', 400)
  try {
    const result = parseUser(await requestJson(`/Usuario/${id}/estado`, { method: 'PATCH', token, body: { estado }, validationMessages: true }))
    if (result.id_usuario !== id || result.estado !== estado) throw new ApiError('No se pudo confirmar el cambio de estado.')
    return result
  } catch (failure) {
    if (failure instanceof ApiError && failure.status === 404) throw new ApiError('El usuario ya no existe. Actualiza la lista.', 404)
    throw failure
  }
}