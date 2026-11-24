import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'

export type CreateSurtidoRequest = {
  nombre: string
  idMarca: number
  idsProductos: number[]
  urlImagen?: string
  imagen?: File
  presentaciones: { idPresentacion: number; unidadesEquivalentes: number; precio: number }[]
}

export type SurtidoSummary = { idSurtido: number; nombre: string; idMarca: number; marca: string; estado: boolean; productos: number;
  imagen: string | null; presentaciones: { nombre: string | null; unidadesEquivalentes: number; precio: number }[] }
export type SurtidoPage = { total: number; pagina: number; tamano: number; elementos: SurtidoSummary[] }
const record = (value: unknown): value is Record<string, unknown> => typeof value === 'object' && value !== null
export type SurtidoDetail = {
  idSurtido: number; nombre: string; idMarca: number; marca: string; estado: boolean; imagen: string | null
  productos: { idProducto: number; nombre: string; costoPromedio: number | null }[]
  presentaciones: { idProductoPresentacion: number; idPresentacion: number; presentacion: string | null; unidadesEquivalentes: number; precio: number; precioMinimoActual: number | null }[]
}
export async function getSurtido(id: number, token: string, signal?: AbortSignal): Promise<SurtidoDetail> {
  const result = await requestJson(`/surtidos/${id}`, { token, signal })
  if (!record(result) || result.idSurtido !== id || !Number.isSafeInteger(result.idMarca) || typeof result.nombre !== 'string' || typeof result.marca !== 'string' ||
      typeof result.estado !== 'boolean' || !(result.imagen === null || typeof result.imagen === 'string') ||
      !Array.isArray(result.productos) || !result.productos.every((p: unknown) => record(p) && Number.isSafeInteger(p.idProducto) && typeof p.nombre === 'string' &&
        (p.costoPromedio === null || typeof p.costoPromedio === 'number')) ||
      !Array.isArray(result.presentaciones) || !result.presentaciones.every((p: unknown) => record(p) && Number.isSafeInteger(p.idProductoPresentacion) && Number.isSafeInteger(p.idPresentacion) &&
        (p.presentacion === null || typeof p.presentacion === 'string') && Number.isSafeInteger(p.unidadesEquivalentes) && typeof p.precio === 'number' &&
        (p.precioMinimoActual === null || typeof p.precioMinimoActual === 'number')))
    throw new ApiError('El servidor devolvió detalles de surtido inválidos.')
  return result as SurtidoDetail
}
export async function listSurtidos(text: string, page: number, token: string, signal?: AbortSignal, idMarca?: number): Promise<SurtidoPage> {
  const params = new URLSearchParams({ texto: text.trim(), pagina: String(page), tamano: '20' })
  if (idMarca !== undefined) params.set('idMarca', String(idMarca))
  const result = await requestJson(`/surtidos?${params}`, { token, signal })
  if (!record(result) || result.pagina !== page || result.tamano !== 20 || !Number.isSafeInteger(result.total) || Number(result.total) < 0 ||
      !Array.isArray(result.elementos) || result.elementos.length > 20 || !result.elementos.every((s: unknown) =>
        record(s) && Number.isSafeInteger(s.idSurtido) && Number(s.idSurtido) > 0 && typeof s.nombre === 'string' &&
        Number.isSafeInteger(s.idMarca) && Number(s.idMarca) > 0 && typeof s.marca === 'string' && typeof s.estado === 'boolean' &&
        Number.isSafeInteger(s.productos) && Number(s.productos) >= 0 && (s.imagen === null || typeof s.imagen === 'string') &&
        Array.isArray(s.presentaciones) && s.presentaciones.every((p: unknown) => record(p) && (p.nombre === null || typeof p.nombre === 'string') &&
          Number.isSafeInteger(p.unidadesEquivalentes) && Number(p.unidadesEquivalentes) > 0 && typeof p.precio === 'number' && Number.isFinite(p.precio))))
    throw new ApiError('El servidor devolvió un listado de surtidos inválido.')
  return result as SurtidoPage
}

export async function createSurtido(data: CreateSurtidoRequest, token: string): Promise<number> {
  return saveSurtido(data, token)
}
export async function updateSurtido(id: number, data: CreateSurtidoRequest & { quitarImagen: boolean; estado: boolean }, token: string): Promise<number> {
  return saveSurtido(data, token, id, data.quitarImagen, data.estado)
}
async function saveSurtido(data: CreateSurtidoRequest, token: string, id?: number, quitarImagen = false, estado = true): Promise<number> {
  let body: unknown = { nombre: data.nombre, idMarca: data.idMarca, idsProductos: data.idsProductos, presentaciones: data.presentaciones,
    ...(data.urlImagen?.trim() ? { urlImagen: data.urlImagen.trim() } : {}),
    ...(id !== undefined ? { quitarImagen, estado } : {}) }
  if (data.imagen) {
    const form = new FormData()
    form.append('Nombre', data.nombre); form.append('IdMarca', String(data.idMarca)); form.append('Imagen', data.imagen)
    if (id !== undefined) { form.append('QuitarImagen', String(quitarImagen)); form.append('Estado', String(estado)) }
    if (data.urlImagen?.trim()) form.append('UrlImagen', data.urlImagen.trim())
    data.idsProductos.forEach((id, i) => form.append(`IdsProductos[${i}]`, String(id)))
    data.presentaciones.forEach((p, i) => Object.entries(p).forEach(([key, value]) => form.append(`Presentaciones[${i}].${key}`, String(value))))
    body = form
  }
  const result = await requestJson('/surtidos' + (id !== undefined ? `/${id}` : '') + (data.imagen ? '/con-imagen' : ''), {
    method: id !== undefined ? 'PUT' : 'POST', body, token, validationMessages: true,
  })
  if (!result || typeof result !== 'object' || !('idSurtido' in result) ||
      !Number.isSafeInteger(result.idSurtido) || Number(result.idSurtido) <= 0 || (id !== undefined && result.idSurtido !== id))
    throw new ApiError('No se pudo confirmar el registro. Verifica si se guardó antes de reintentar.')
  return Number(result.idSurtido)
}
