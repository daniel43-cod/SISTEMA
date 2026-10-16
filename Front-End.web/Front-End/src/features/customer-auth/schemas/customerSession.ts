import type { CustomerCredentials, CustomerSession } from '../types/customerAuth.ts'

const record = (v: unknown): v is Record<string, unknown> => !!v && typeof v === 'object'
export function validateCustomerCredentials(credentials: CustomerCredentials): string | null {
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(credentials.correoElectronico.trim())) return 'Ingresa un correo electrónico válido.'
  if (!credentials.password) return 'Ingresa tu contraseña.'
  if (new TextEncoder().encode(credentials.password).length > 72) return 'La contraseña no puede superar 72 bytes.'
  return null
}
export function parseCustomerSession(value: unknown): CustomerSession {
  if (!record(value) || typeof value.idCuentaCliente !== 'number' || !Number.isSafeInteger(value.idCuentaCliente) ||
      value.idCuentaCliente <= 0 || typeof value.correoElectronico !== 'string' || !value.correoElectronico.trim() ||
      typeof value.token !== 'string') throw new Error('La respuesta de sesión del cliente no es válida.')
  try {
    const parts = value.token.split('.')
    if (parts.length !== 3 || parts.some(part => !part)) throw new Error()
    const encoded = parts[1].replace(/-/g, '+').replace(/_/g, '/')
    const bytes = Uint8Array.from(atob(encoded.padEnd(Math.ceil(encoded.length / 4) * 4, '=')), c => c.charCodeAt(0))
    const payload: unknown = JSON.parse(new TextDecoder().decode(bytes))
    if (!record(payload) || payload.tipo_cuenta !== 'cuenta_cliente' || payload.sub !== String(value.idCuentaCliente) ||
        typeof payload.exp !== 'number' || !Number.isSafeInteger(payload.exp) ||
        !Number.isSafeInteger(payload.exp * 1000) || payload.exp * 1000 <= Date.now()) throw new Error()
    // La firma y los permisos se verifican en la API. Aquí controlamos el estado de la interfaz.
    return { idCuentaCliente: value.idCuentaCliente, correoElectronico: value.correoElectronico,
      token: value.token, expiresAt: payload.exp * 1000 }
  } catch { throw new Error('La sesión del cliente no es válida o ya venció.') }
}
