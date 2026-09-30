import type { LoginCredentials, StaffSession } from '../types/auth.ts'

export type LoginErrors = Partial<Record<keyof LoginCredentials, string>>

//validar que las credenciales tengan los formatos correctos para mandrlos al back
export function validateLogin(credentials: LoginCredentials): LoginErrors {
  const errors: LoginErrors = {}
  if (!credentials.usuario.trim()) errors.usuario = 'Ingresa tu usuario.'
  else if (credentials.usuario.length > 50) errors.usuario = 'El usuario admite hasta 50 caracteres.'
  if (!credentials.password) errors.password = 'Ingresa tu contraseña.'
  else if (new TextEncoder().encode(credentials.password).length > 72)
    errors.password = 'La contraseña no puede superar 72 bytes.'
  return errors
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null
}

export function parseStaffSession(value: unknown): StaffSession {
  if (!isRecord(value) || !Number.isSafeInteger(value.id_usuario) || Number(value.id_usuario) <= 0 ||
    typeof value.nombre !== 'string' || !value.nombre.trim() ||
    (value.rol !== 'ADMINISTRADOR' && value.rol !== 'VENDEDOR') || typeof value.token !== 'string')
    throw new Error('Respuesta de inicio de sesión inválida.')
  try {
    const parts = value.token.split('.')
    if (parts.length !== 3 || parts.some(part => !part)) throw new Error()
    const encoded = parts[1].replace(/-/g, '+').replace(/_/g, '/')
    const bytes = Uint8Array.from(atob(encoded.padEnd(Math.ceil(encoded.length / 4) * 4, '=')), c => c.charCodeAt(0))
    const payload: unknown = JSON.parse(new TextDecoder().decode(bytes))
    if (!isRecord(payload) || typeof payload.exp !== 'number' || !Number.isSafeInteger(payload.exp) ||
      payload.exp * 1000 <= Date.now() || payload.exp * 1000 > Date.now() + 31 * 60 * 1000 ||
      payload.tipo_cuenta !== 'usuario' || payload.sub !== String(value.id_usuario)) throw new Error()
    // Solo programamos el vencimiento de la UI. La firma y permisos los valida la API.
    return {
      user: { id: Number(value.id_usuario), name: value.nombre, role: value.rol },
      token: value.token, expiresAt: payload.exp * 1000,
    }
  } catch { throw new Error('La respuesta de sesión no es válida o ya venció.') }
}