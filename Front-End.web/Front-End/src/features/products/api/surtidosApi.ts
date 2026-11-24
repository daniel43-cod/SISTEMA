import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'

export type CreateSurtidoRequest = {
  nombre: string
  idMarca: number
  idsProductos: number[]
  presentaciones: { idPresentacion: number; unidadesEquivalentes: number; precio: number }[]
}

export async function createSurtido(data: CreateSurtidoRequest, token: string): Promise<number> {
  const result = await requestJson('/surtidos', { method: 'POST', body: data, token, validationMessages: true })
  if (!result || typeof result !== 'object' || !('idSurtido' in result) ||
      !Number.isSafeInteger(result.idSurtido) || Number(result.idSurtido) <= 0)
    throw new ApiError('No se pudo confirmar el registro. Verifica si se guardó antes de reintentar.')
  return Number(result.idSurtido)
}
