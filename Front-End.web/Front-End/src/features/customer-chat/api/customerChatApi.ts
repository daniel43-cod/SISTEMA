import { requestJson } from '../../../shared/api/httpClient'
import { ApiError } from '../../../shared/api/ApiError'
import { API_BASE_URL } from '../../../shared/config/api'

export interface Chat { idConversacion: number; contenido: string }
export interface ChatMessage { idMensaje: number; mensaje: string; respuesta: string | null }
const record = (v: unknown): v is Record<string, unknown> => !!v && typeof v === 'object'
const id = (v: unknown): v is number => typeof v === 'number' && Number.isSafeInteger(v) && v > 0
export function parseChats(value: unknown): Chat[] {
  if (!Array.isArray(value) || !value.every(row => record(row) && id(row.idConversacion) && typeof row.contenido === 'string') ||
      new Set(value.map(row => row.idConversacion)).size !== value.length) throw new ApiError('El historial de chats no es válido.')
  return value
}
export function parseMessages(value: unknown): ChatMessage[] {
  if (!Array.isArray(value) || !value.every(row => record(row) && id(row.idMensaje) && typeof row.mensaje === 'string' &&
      (row.respuesta === null || typeof row.respuesta === 'string')) ||
      new Set(value.map(row => row.idMensaje)).size !== value.length) throw new ApiError('El historial de mensajes no es válido.')
  return value
}
export async function listChats(token: string, page = 1, signal?: AbortSignal) {
  return parseChats(await requestJson(`/Conversaciones?pagina=${page}&tamanoPagina=25`, { token, signal }))
}
export async function listMessages(token: string, conversationId: number, before?: number, signal?: AbortSignal) {
  return parseMessages(await requestJson(`/Conversaciones/${conversationId}/mensajes?limite=50${before ? `&antesDe=${before}` : ''}`, { token, signal }))
}
export class ChatSendError extends ApiError {
  conversationId: number | null
  constructor(message: string, status: number, conversationId: number | null = null) {
    super(message, status)
    this.conversationId = conversationId
  }
}
export async function sendChat(token: string, text: string, conversationId: number | null) {
  if (!text.trim() || text.length > 2000) throw new ChatSendError('Escribe entre 1 y 2000 caracteres.', 400)
  // La respuesta de IA puede tardar más que una consulta normal. No reintentamos un POST automáticamente.
  let response: Response
  try {
    response = await fetch(API_BASE_URL + '/Mensajes', {
      method: 'POST', headers: { 'Content-Type': 'application/json', Accept: 'application/json', Authorization: `Bearer ${token}` },
      body: JSON.stringify({ mensaje: text.trim(), idConversacion: conversationId }),
      signal: AbortSignal.timeout(90000), credentials: 'omit', cache: 'no-store', redirect: 'error',
    })
  } catch { throw new ChatSendError('No se pudo confirmar el envío. Actualiza tus chats antes de volver a enviar para evitar duplicarlo.', 0) }
  let data: unknown
  try { data = await response.json() } catch { data = null }
  if (!response.ok) {
    const savedId = record(data) && id(data.idConversacion) ? data.idConversacion : null
    const message = response.status === 401 || response.status === 403 ? 'La sesión del cliente ya no está disponible.'
      : savedId ? 'Tu pregunta quedó guardada, pero la IA no pudo responder. Puedes continuar en este chat.'
      : response.status === 429 ? 'Demasiados mensajes. Espera antes de volver a intentar.'
      : response.status === 400 ? 'Revisa el mensaje; admite hasta 2000 caracteres.'
      : 'No se pudo confirmar el envío. Actualiza el historial antes de volver a enviar.'
    throw new ChatSendError(message, response.status, savedId)
  }
  if (!record(data) || !id(data.idMensaje) || !id(data.idConversacion) ||
      typeof data.respuestaMensaje !== 'string' || (conversationId !== null && data.idConversacion !== conversationId))
    throw new ChatSendError('No se pudo interpretar la respuesta. Actualiza el historial antes de volver a enviar.', 0)
  return { idMensaje: data.idMensaje, idConversacion: data.idConversacion, respuesta: data.respuestaMensaje }
}
