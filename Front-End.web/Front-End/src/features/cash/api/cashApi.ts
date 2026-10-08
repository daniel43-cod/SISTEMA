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

export type CashMovement = {
  id_movimiento_caja: number; id_sesion_caja: number; tipo_movimiento: string
  naturaleza: string; usuario: string; fecha_movimiento: string; monto: number; descripcion: string | null
}
export type CashSummary = {
  id_sesion_caja: number; monto_inicial: number; total_entradas: number; total_salidas: number
  saldo_esperado: number; total_movimientos: number; movimientos: CashMovement[]
}

// Los totales proceden del servidor: los 100 movimientos visibles no representan toda la sesion.
export function parseCashSummary(value: unknown, idSession: number): CashSummary {
  const finite = (item: unknown): item is number => typeof item === 'number' && Number.isFinite(item)
  if (!record(value) || !positiveId(value.id_sesion_caja) || value.id_sesion_caja !== idSession ||
      !finite(value.monto_inicial) || value.monto_inicial < 0 ||
      !finite(value.total_entradas) || value.total_entradas < 0 ||
      !finite(value.total_salidas) || value.total_salidas < 0 || !finite(value.saldo_esperado) ||
      typeof value.total_movimientos !== 'number' || !Number.isSafeInteger(value.total_movimientos) || value.total_movimientos < 0 ||
      !Array.isArray(value.movimientos) || value.movimientos.length !== Math.min(100, value.total_movimientos) ||
      value.movimientos.some(m => !record(m) || !positiveId(m.id_movimiento_caja) || m.id_sesion_caja !== idSession ||
        typeof m.tipo_movimiento !== 'string' || typeof m.naturaleza !== 'string' ||
        !['ENTRADA', 'INGRESO', 'SALIDA', 'EGRESO'].includes(m.naturaleza.trim().toUpperCase()) ||
        typeof m.usuario !== 'string' || typeof m.fecha_movimiento !== 'string' || !Number.isFinite(Date.parse(m.fecha_movimiento)) ||
        !finite(m.monto) || m.monto <= 0 || (m.descripcion !== null && typeof m.descripcion !== 'string')))
    throw new ApiError('No se pudo interpretar el resumen de caja.')
  if (new Set(value.movimientos.map(m => m.id_movimiento_caja)).size !== value.movimientos.length)
    throw new ApiError('El resumen de caja contiene movimientos duplicados.')
  return value as CashSummary
}

export async function loadCashSummary(token: string, idSession: number, signal?: AbortSignal): Promise<CashSummary> {
  if (!positiveId(idSession)) throw new ApiError('El turno de caja no es válido.')
  return parseCashSummary(await requestJson('/MovimientoCaja/listar?idSesionCaja=' + idSession, { token, signal }), idSession)
}
