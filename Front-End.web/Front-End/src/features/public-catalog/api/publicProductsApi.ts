import { requestJson } from '../../../shared/api/httpClient'
import { ApiError } from '../../../shared/api/ApiError'
import type { PublicProduct, PublicPresentation, PublicProductsPage } from '../types/product'

export async function listPublicProducts(idMarca: number, pagina: number, signal?: AbortSignal): Promise<PublicProductsPage> {
  if (!Number.isSafeInteger(idMarca) || idMarca <= 0 || !Number.isSafeInteger(pagina) || pagina < 1)
    throw new ApiError('La marca o página seleccionada no es válida.', 400)
  let response: unknown
  try {
    response = await requestJson(`/CatalogoPublico/marcas/${idMarca}/productos?pagina=${pagina}&tamanoPagina=24`, { signal })
  } catch (failure) {
    if (failure instanceof ApiError && failure.status === 404)
      throw new ApiError('La marca ya no está disponible. Vuelve a las marcas.', 404)
    throw failure
  }
  const invalid = () => new ApiError('El listado de productos recibido no es válido.')
  if (!response || typeof response !== 'object') throw invalid()
  const page = response as Record<string, unknown>
  if (page.pagina !== pagina || page.tamanoPagina !== 24 || typeof page.hayMas !== 'boolean' ||
      !Array.isArray(page.productos) || page.productos.length > 24) throw invalid()
  const presentationIds = new Set<number>()
  const productos = page.productos.map((value: unknown): PublicProduct => {
    if (!value || typeof value !== 'object') throw invalid()
    const p = value as Record<string, unknown>
    if (typeof p.idProducto !== 'number' || !Number.isSafeInteger(p.idProducto) || p.idProducto <= 0 ||
        typeof p.nombre !== 'string' || !p.nombre.trim() ||
        (p.urlImagen !== null && typeof p.urlImagen !== 'string') ||
        !Array.isArray(p.presentaciones) || !p.presentaciones.length) throw invalid()
    const presentaciones = p.presentaciones.map((value: unknown): PublicPresentation => {
      if (!value || typeof value !== 'object') throw invalid()
      const row = value as Record<string, unknown>
      if (typeof row.idProductoPresentacion !== 'number' || !Number.isSafeInteger(row.idProductoPresentacion) ||
          row.idProductoPresentacion <= 0 || presentationIds.has(row.idProductoPresentacion) ||
          typeof row.nombre !== 'string' || !row.nombre.trim() ||
          typeof row.precio !== 'number' || !Number.isFinite(row.precio) || row.precio < 0 ||
          typeof row.disponible !== 'boolean') throw invalid()
      presentationIds.add(row.idProductoPresentacion)
      return { idProductoPresentacion: row.idProductoPresentacion, nombre: row.nombre,
        precio: row.precio, disponible: row.disponible }
    })
    return { idProducto: p.idProducto, nombre: p.nombre, urlImagen: p.urlImagen, presentaciones }
  })
  if (new Set(productos.map(p => p.idProducto)).size !== productos.length) throw invalid()
  return { pagina, tamanoPagina: 24, hayMas: page.hayMas, productos }
}
