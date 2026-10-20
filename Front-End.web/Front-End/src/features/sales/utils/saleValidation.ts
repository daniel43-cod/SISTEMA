export type SalePresentation = { id_producto_presentacion: number; presentacion: string; unidades_equivalentes: number; precio: number }
export type SaleProduct = { id_producto: number; nombre: string; stock: number; presentaciones: SalePresentation[] }
export type SaleLine = { product: SaleProduct; presentation: SalePresentation; quantity: string; discount?: string }

export function cashCents(value: string): number | null {
  if (!/^\d{1,8}([.,]\d{1,2})?$/.test(value.trim())) return null
  return Math.round(Number(value.trim().replace(',', '.')) * 100)
}

export function saleLineTotal(line: SaleLine): number {
  if (!/^\d+$/.test(line.quantity) || !Number.isSafeInteger(Number(line.quantity)) || Number(line.quantity) < 1)
    throw new Error('Ingresa cantidades enteras mayores a cero.')
  const gross = Math.round(line.presentation.precio * 100) * Number(line.quantity)
  if (!Number.isSafeInteger(gross) || gross > 9999999999) throw new Error('El importe de la línea excede el límite permitido.')
  const discount = cashCents(line.discount ?? '0.00')
  if (discount === null) throw new Error('El descuento debe ser un importe no negativo con máximo dos decimales.')
  if (discount >= gross) throw new Error('El descuento debe ser menor al importe de la línea.')
  return gross - discount
}

export function saleTotals(lines: SaleLine[]) {
  if (!lines.length) throw new Error('Agrega al menos un producto.')
  const used = new Map<number, number>()
  let total = 0
  let subtotal = 0
  for (const line of lines) {
    if (!/^\d+$/.test(line.quantity) || !Number.isSafeInteger(Number(line.quantity)) || Number(line.quantity) < 1)
      throw new Error('Ingresa cantidades enteras mayores a cero.')
    const quantity = Number(line.quantity)
    const units = (used.get(line.product.id_producto) ?? 0) + quantity * line.presentation.unidades_equivalentes
    if (!Number.isSafeInteger(units) || units > line.product.stock) throw new Error(`Existencias insuficientes para ${line.product.nombre}.`)
    used.set(line.product.id_producto, units)
    total += saleLineTotal(line)
    subtotal += Math.round(line.presentation.precio * 100) * quantity
    if (!Number.isSafeInteger(subtotal) || subtotal > 9999999999) throw new Error('El total excede el límite permitido.')
  }
  return total
}
