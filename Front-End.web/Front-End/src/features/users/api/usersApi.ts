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
