export interface ExpenseRow {
  idGasto: number
  descripcion: string
  monto: number
  observacion: string | null
  encargado: string
  fecha: string
}
export interface ExpensePage {
  idSesionCaja: number | null
  pagina: number
  tamanoPagina: number
  total: number
  items: ExpenseRow[]
}
export function parseExpensePage(value: unknown): ExpensePage {
  if (!value || typeof value !== 'object') throw new Error('El listado de gastos no es válido.')
  const page = value as Record<string, unknown>
  const positive = (v: unknown): v is number => typeof v === 'number' && Number.isSafeInteger(v) && v > 0
  if ((page.idSesionCaja !== null && !positive(page.idSesionCaja)) || !positive(page.pagina) ||
      !positive(page.tamanoPagina) || typeof page.total !== 'number' || !Number.isSafeInteger(page.total) ||
      page.total < 0 || !Array.isArray(page.items) || page.items.length > page.tamanoPagina) throw new Error('El listado de gastos no es válido.')
  const items = page.items.map((value: unknown): ExpenseRow => {
    if (!value || typeof value !== 'object') throw new Error('El gasto recibido no es válido.')
    const row = value as Record<string, unknown>
    if (!positive(row.idGasto) || typeof row.descripcion !== 'string' ||
        typeof row.monto !== 'number' || !Number.isFinite(row.monto) || row.monto <= 0 ||
        (row.observacion !== null && typeof row.observacion !== 'string') ||
        typeof row.encargado !== 'string' || typeof row.fecha !== 'string' || !Number.isFinite(Date.parse(row.fecha)))
      throw new Error('El gasto recibido no es válido.')
    return { idGasto: row.idGasto, descripcion: row.descripcion, monto: row.monto,
      observacion: row.observacion, encargado: row.encargado, fecha: row.fecha }
  })
  if (new Set(items.map(row => row.idGasto)).size !== items.length) throw new Error('El listado contiene gastos duplicados.')
  return { idSesionCaja: page.idSesionCaja, pagina: page.pagina, tamanoPagina: page.tamanoPagina, total: page.total, items }
}

