import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'

export type AvailableCash = { idCaja: number; nombre: string }
export type CashSession = {
  id_sesion_caja: number; id_caja: number; usuario_apertura: string
  fecha_apertura: string; monto_inicial: number
}
const record = (value: unknown): value is Record<string, unknown> => typeof value === 'object' && value !== null
const positiveId = (value: unknown): value is number => typeof value === 'number' && Number.isSafeInteger(value) && value > 0

// Verifica el contrato antes de mostrar datos o habilitar una nueva apertura.
export function parseCashSession(value: unknown): CashSession | null {
  if (value === null) return null
  if (!record(value) || !positiveId(value.id_sesion_caja) || !positiveId(value.id_caja) ||
      typeof value.usuario_apertura !== 'string' || typeof value.fecha_apertura !== 'string' ||
      !Number.isFinite(Date.parse(value.fecha_apertura)) || typeof value.monto_inicial !== 'number' ||
      !Number.isFinite(value.monto_inicial) || value.monto_inicial < 0 || value.fecha_cierre !== null)
    throw new ApiError('No se pudo interpretar el estado de la caja.')
  return value as CashSession
}

export function parseAvailableCash(value: unknown): AvailableCash[] {
  if (!Array.isArray(value) || value.length > 100 || value.some(item => !record(item) ||
      !positiveId(item.idCaja) || typeof item.nombre !== 'string'))
    throw new ApiError('No se pudieron interpretar las cajas disponibles.')
  if (new Set(value.map(item => item.idCaja)).size !== value.length) throw new ApiError('Las cajas disponibles contienen duplicados.')
  return value as AvailableCash[]
}

export async function loadCash(token: string, signal?: AbortSignal) {
  const [current, available] = await Promise.all([
    requestJson('/Caja/actual', { token, signal }),
    requestJson('/Caja/disponibles', { token, signal }),
  ])
  return { current: parseCashSession(current), available: parseAvailableCash(available) }
}

export function parseOpeningAmount(value: string): number | null {
  const normalized = value.trim().replace(',', '.')
  if (!/^\d{1,8}(?:[.]\d{1,2})?$/.test(normalized)) return null
  const amount = Number(normalized)
  return Number.isFinite(amount) && amount <= 99999999.99 ? amount : null
}

export async function openCash(token: string, idCaja: number, amount: number, note: string) {
  await requestJson('/Caja/abrir', { method: 'POST', token, validationMessages: true,
    body: { id_caja: idCaja, monto_inicial: amount, observacion: note.trim() || null } })
}

export type CashClosing = {
  id_sesion_caja: number; id_usuario_apertura: number; id_usuario_cierre: number
  fecha_cierre: string; monto_inicial: number; monto_esperado: number
  monto_contado: number; diferencia: number
}

export function parseCashClosing(value: unknown): CashClosing {
  if (!record(value) || !positiveId(value.id_sesion_caja) || !positiveId(value.id_usuario_apertura) ||
      !positiveId(value.id_usuario_cierre) || typeof value.fecha_cierre !== 'string' ||
      !Number.isFinite(Date.parse(value.fecha_cierre)) ||
      ['monto_inicial', 'monto_esperado', 'monto_contado', 'diferencia'].some(key =>
        typeof value[key] !== 'number' || !Number.isFinite(value[key]) || Math.abs(value[key]) > 99999999.99) ||
      (value.monto_inicial as number) < 0 || (value.monto_contado as number) < 0)
    throw new ApiError('No se pudo interpretar el resultado del cierre. Consulta el estado de la caja.')
  return value as CashClosing
}

export async function closeCash(token: string, idSession: number, amount: number, note: string): Promise<CashClosing> {
  // Se envia el turno revisado, nunca el usuario responsable ni saldos calculados en el navegador.
  if (!positiveId(idSession) || !Number.isFinite(amount) || amount < 0 || amount > 99999999.99 ||
      Math.abs(Math.round(amount * 100) - amount * 100) > 0.00001 || note.length > 100)
    throw new ApiError('Revisa los datos del cierre.')
  const response = parseCashClosing(await requestJson('/Caja/cerrar', { method: 'POST', token, validationMessages: true,
    body: { id_sesion_caja: idSession, monto_contado: amount, observacion_cierre: note.trim() || null } }))
  if (response.id_sesion_caja !== idSession) throw new ApiError('El resultado no corresponde al turno solicitado. Consulta el estado de la caja.')
  return response
}
