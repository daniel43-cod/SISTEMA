import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'
import type { PublicProduct, PublicPresentation, PublicProductsPage } from '../types/product.ts'
import type { PublicSurtido } from '../types/product.ts'

export async function listPublicSurtidos(idMarca: number, pagina: number, signal?: AbortSignal): Promise<{ surtidos: PublicSurtido[]; hayMas: boolean }> {
  const result = await requestJson(`/CatalogoPublico/marcas/${idMarca}/surtidos?pagina=${pagina}&tamanoPagina=24`, { signal })
  if (!result || typeof result !== 'object') throw new ApiError('El listado de surtidos no es válido.')
  const page = result as Record<string, unknown>
  if (page.pagina !== pagina || page.tamanoPagina !== 24 || typeof page.hayMas !== 'boolean' || !Array.isArray(page.surtidos) || page.surtidos.length > 24)
    throw new ApiError('El listado de surtidos no es válido.')
  for (const value of page.surtidos) {
    const s = value as PublicSurtido
    if (!s || !Number.isSafeInteger(s.idSurtido) || s.idSurtido <= 0 || typeof s.nombre !== 'string' || !(s.urlImagen === null || typeof s.urlImagen === 'string') ||
        !Array.isArray(s.productos) || !s.productos.every(p => p && Number.isSafeInteger(p.idProducto) && p.idProducto > 0 && typeof p.nombre === 'string' && typeof p.disponible === 'boolean') ||
        !Array.isArray(s.presentaciones) || !s.presentaciones.every(p => p && Number.isSafeInteger(p.idProductoPresentacion) && p.idProductoPresentacion > 0 && typeof p.nombre === 'string' &&
          Number.isSafeInteger(p.unidadesEquivalentes) && p.unidadesEquivalentes > 0 && typeof p.precio === 'number' && Number.isFinite(p.precio) && p.precio > 0 && typeof p.disponible === 'boolean'))
      throw new ApiError('El listado de surtidos no es válido.')
  }
  return { surtidos: page.surtidos as PublicSurtido[], hayMas: page.hayMas }
}

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
