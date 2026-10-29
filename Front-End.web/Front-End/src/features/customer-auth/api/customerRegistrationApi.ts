import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'
import { validateCustomerRegistration } from '../schemas/customerRegistration.ts'

export async function registerCustomer(email: string, password: string, confirmation: string, signal?: AbortSignal): Promise<void> {
  const validation = validateCustomerRegistration(email, password, confirmation)
  if (validation) throw new ApiError(validation, 400)
  try {
    const result = await requestJson('/CuentaCliente/registro', { method: 'POST', signal,
      body: { correoElectronico: email.trim().toLowerCase(), password } })
    if (!result || typeof result !== 'object' || !('idCuentaCliente' in result) ||
      !Number.isSafeInteger(result.idCuentaCliente) || Number(result.idCuentaCliente) <= 0)
      throw new ApiError('No se pudo confirmar el registro. Intenta iniciar sesión o verifica antes de volver a crear la cuenta.', 200)
  } catch (failure) {
    if (failure instanceof ApiError && failure.status === 409)
      throw new ApiError('Ese correo ya tiene una cuenta. Inicia sesión para continuar.', 409)
    throw failure
  }
}
