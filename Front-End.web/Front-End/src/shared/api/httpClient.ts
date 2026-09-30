import { API_BASE_URL } from '../config/api.ts'
import { ApiError } from './ApiError.ts'

type RequestOptions = {
  method?: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE'
  body?: unknown
  token?: string
  signal?: AbortSignal
}

export function parseRetryAfter(value: string | null): number {
  if (!value) return 60
  const seconds = /^\d+$/.test(value) ? Number(value) : (Date.parse(value) - Date.now()) / 1000
  return Number.isFinite(seconds) ? Math.max(1, Math.ceil(seconds)) : 60
}

export async function requestJson(path: string, options: RequestOptions = {}): Promise<unknown> {
  if (!path.startsWith('/') || path.startsWith('//') || path.includes('..') || path.includes('\\'))
    throw new Error('Ruta API inválida.')
  const headers: Record<string, string> = { Accept: 'application/json' }
  if (options.body !== undefined) headers['Content-Type'] = 'application/json'
  if (options.token) headers.Authorization = `Bearer ${options.token}`
  const signal = options.signal
    ? AbortSignal.any([options.signal, AbortSignal.timeout(15000)])
    : AbortSignal.timeout(15000)
  try {
    const response = await fetch(API_BASE_URL + path, {
      method: options.method ?? 'GET',
      headers, body: options.body === undefined ? undefined : JSON.stringify(options.body),
      signal, cache: 'no-store', credentials: 'omit', redirect: 'error',
    })
    if (!response.ok) {
      const messages: Record<number, string> = {
        400: 'Revisa los datos ingresados.',
        401: 'La sesión no es válida. Inicia sesión nuevamente.',
        403: 'No tienes permiso para realizar esta acción.',
        429: 'Demasiados intentos. Espera antes de volver a intentarlo.',
      }
      // No mostrar mensajes internos del servidor sin un contrato explícito.
      throw new ApiError(messages[response.status] ?? 'No se pudo completar la solicitud. Inténtalo más tarde.',
        response.status, response.status === 429 ? parseRetryAfter(response.headers.get('Retry-After')) : null)
    }
    if (response.status === 204) return null
    try { return await response.json() }
    catch { throw new ApiError('El servidor devolvió una respuesta inválida.', response.status) }
  } catch (error) {
    if (error instanceof ApiError) throw error
    if (options.signal?.aborted) throw error
    throw new ApiError(signal.aborted
      ? 'La solicitud tardó demasiado. Inténtalo nuevamente.'
      : 'No se pudo conectar con el servidor. Revisa tu conexión e inténtalo nuevamente.')
  }
}