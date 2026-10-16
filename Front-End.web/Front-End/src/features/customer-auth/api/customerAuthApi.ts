import { requestJson } from '../../../shared/api/httpClient'
import { ApiError } from '../../../shared/api/ApiError'
import { parseCustomerSession, validateCustomerCredentials } from '../schemas/customerSession'
import type { CustomerCredentials } from '../types/customerAuth'

export async function loginCustomer(credentials: CustomerCredentials, signal?: AbortSignal) {
  const validation = validateCustomerCredentials(credentials)
  if (validation) throw new ApiError(validation, 400)
  try {
    const result = await requestJson('/CuentaCliente/iniciosesion', {
      method: 'POST', signal,
      body: { correoElectronico: credentials.correoElectronico.trim(), password: credentials.password },
    })
    try { return parseCustomerSession(result) }
    catch { throw new ApiError('El servidor devolvió una sesión de cliente inválida. Inténtalo nuevamente.') }
  } catch (failure) {
    if (failure instanceof ApiError && failure.status === 401)
      throw new ApiError('Correo o contraseña incorrectos, o cuenta no disponible.', 401)
    throw failure
  }
}
