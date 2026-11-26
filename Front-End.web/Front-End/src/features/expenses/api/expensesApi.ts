import { parseExpensePage } from '../utils/expenseListing.ts'
import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'
import { parseCashSession } from '../../cash/api/cashApi.ts'
import type { buildExpense } from '../utils/expenseValidation.ts'

export async function checkExpenseCash(token: string, signal?: AbortSignal) {
  return parseCashSession(await requestJson('/Caja/actual', { token, signal }))
}
export async function createExpense(token: string, body: ReturnType<typeof buildExpense> & { id_operacion: string }) {
  const response = await requestJson('/Gastos/crear', { method: 'POST', token, body, validationMessages: true })
  if (!response || typeof response !== 'object' || !('id_gasto' in response) ||
      typeof response.id_gasto !== 'number' || !Number.isSafeInteger(response.id_gasto) || response.id_gasto <= 0)
    throw new ApiError('No se pudo confirmar el registro. Revisa los movimientos de caja antes de volver a enviarlo.')
  return response.id_gasto
}

export async function listExpenses(token: string, page = 1, signal?: AbortSignal, idUsuarioFiltro?: number) {
  const response = await requestJson(`/Gastos/listar?pagina=${page}&tamanoPagina=25` + (idUsuarioFiltro ? "&idUsuarioFiltro=" + idUsuarioFiltro : ""), { token, signal })
  try { return parseExpensePage(response) }
  catch { throw new ApiError('El listado de gastos recibido no es válido.') }
}