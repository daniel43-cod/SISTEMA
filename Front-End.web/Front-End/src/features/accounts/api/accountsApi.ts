import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'

export type AccountType = 'cobrar' | 'pagar'
export type Account = { id: number; nombre: string; usuario: string; fecha: string; total: number; saldo: number; subtotal: number; descuento: number | null; ganancia: number | null }
export type AccountsPage = { totalPorCobrar: number; totalPorPagar: number; totalRegistros: number; saldoFiltrado: number; hayMas: boolean; items: Account[] }
const object = (v: unknown): v is Record<string, unknown> => !!v && typeof v === 'object'
const amount = (v: unknown): v is number => typeof v === 'number' && Number.isFinite(v) && v >= 0
export function parseAccounts(value: unknown): AccountsPage {
  if (!object(value) || !amount(value.totalPorCobrar) || !amount(value.totalPorPagar) || !amount(value.saldoFiltrado) ||
    !Number.isSafeInteger(value.totalRegistros) || Number(value.totalRegistros) < 0 || typeof value.hayMas !== 'boolean' ||
    !Array.isArray(value.items) || value.items.length > 20 || !value.items.every(v => object(v) &&
      Number.isSafeInteger(v.id) && Number(v.id) > 0 && typeof v.nombre === 'string' && typeof v.usuario === 'string' &&
      typeof v.fecha === 'string' && Number.isFinite(Date.parse(v.fecha)) && amount(v.total) && amount(v.saldo) && v.saldo > 0 && v.saldo <= v.total &&
      amount(v.subtotal) && (v.descuento === null || amount(v.descuento)) && (v.ganancia === null || (typeof v.ganancia === 'number' && Number.isFinite(v.ganancia)))) ||
    new Set(value.items.map(v => v.id)).size !== value.items.length)
    throw new ApiError('La respuesta de cuentas no tiene el formato esperado.')
  return value as AccountsPage
}
export async function loadAccounts(token: string, tipo: AccountType, pagina: number, buscar: string, signal?: AbortSignal) {
  const query = new URLSearchParams({ tipo, pagina: String(pagina), tamanoPagina: '20', buscar })
  return parseAccounts(await requestJson('/Cuentas?' + query, { token, signal }))
}
