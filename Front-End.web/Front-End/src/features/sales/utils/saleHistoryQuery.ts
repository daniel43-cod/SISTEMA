export function saleHistoryQuery(page: number, range?: { desde: string; hasta: string }, userId?: number, pageSize = 50) {
  if (userId !== undefined && (!Number.isSafeInteger(userId) || userId <= 0)) throw new Error('Usuario inválido.')
  const params = new URLSearchParams({ pagina: String(page), tamanoPagina: String(pageSize) })
  if (range) {
    params.set('desde', range.desde); params.set('hasta', range.hasta)
    if (userId !== undefined) params.set('idUsuario', String(userId))
    return '/Venta/historial?' + params
  }
  return (userId === undefined ? '/Venta/listar' : '/Venta/caja-activa/usuario/' + userId) + '?' + params
}
