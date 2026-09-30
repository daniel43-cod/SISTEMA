import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'
import { parseStaffSession } from '../schemas/loginSchema.ts'
import type { LoginCredentials, StaffSession } from '../types/auth.ts'

export async function loginStaff(credentials: LoginCredentials, signal?: AbortSignal): Promise<StaffSession> {
  try {
    const data = await requestJson('/Login/Login', {
      method: 'POST', body: { usuario: credentials.usuario.trim(), password: credentials.password }, signal,
    })
    try { return parseStaffSession(data) }
    catch { throw new ApiError('El servidor devolvió una sesión inválida. Inténtalo nuevamente.') }
  } catch (error) {
    if (error instanceof ApiError && error.status === 401)
      throw new ApiError('Usuario o contraseña incorrectos.', 401)
    throw error
  }
}