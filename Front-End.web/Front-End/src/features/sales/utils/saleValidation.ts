export type SalePresentation = { id_producto_presentacion: number; presentacion: string; unidades_equivalentes: number; precio: number }
export type SaleProduct = { id_producto: number; id_surtido?: number; bloqueo?: string | null; nombre: string; stock: number; presentaciones: SalePresentation[];
  componentes?: { id_producto: number; nombre: string; stock: number }[] }
export type SaleComponent = { id_producto: number; cantidad: number }
export type SaleLine = { product: SaleProduct; presentation: SalePresentation; quantity: string; discount?: string; componentes?: SaleComponent[] }

export function surtidoLineFromInputs(product: SaleProduct, presentation: SalePresentation, quantity: string, discount: string, values: Record<number, string>): SaleLine {
  if (!product.id_surtido) throw new Error('Selecciona un surtido.')
  if (Object.entries(values).some(([id, value]) => !product.componentes?.some(p => p.id_producto === Number(id)) ||
      (value !== '' && (!/^\d+$/.test(value) || !Number.isSafeInteger(Number(value)) || Number(value) > 2147483647))))
    throw new Error('Las cantidades de los productos deben ser números enteros no negativos.')
  const line: SaleLine = { product, presentation, quantity, discount,
    componentes: (product.componentes ?? []).map(c => ({ id_producto: c.id_producto, cantidad: Number(values[c.id_producto] || '0') })).filter(c => c.cantidad > 0) }
  saleLineTotal(line)
  return line
}

export function validateSurtidoComposition(line: SaleLine): void {
  if (!line.product.id_surtido) return
  if (line.product.bloqueo) throw new Error(line.product.bloqueo)
  const components = line.componentes
  const required = Number(line.quantity) * line.presentation.unidades_equivalentes
  if (!Number.isSafeInteger(required) || required < 1 || required > 2147483647) throw new Error('Revisa la cantidad de unidades del surtido.')
  if (!components?.length || new Set(components.map(c => c.id_producto)).size !== components.length || components.some(c =>
    !Number.isSafeInteger(c.cantidad) || c.cantidad < 1 || c.cantidad > 2147483647 || !line.product.componentes?.some(p => p.id_producto === c.id_producto)))
    throw new Error('Ingresa cantidades enteras de los productos permitidos, sin repetirlos.')
  if (components.reduce((sum, c) => sum + c.cantidad, 0) !== required) throw new Error(`Completa exactamente ${required} unidades del surtido.`)
}

export function cashCents(value: string): number | null {
  if (!/^\d{1,8}([.,]\d{1,2})?$/.test(value.trim())) return null
  return Math.round(Number(value.trim().replace(',', '.')) * 100)
}

export function saleLineTotal(line: SaleLine): number {
  if (!/^\d+$/.test(line.quantity) || !Number.isSafeInteger(Number(line.quantity)) || Number(line.quantity) < 1)
    throw new Error('Ingresa cantidades enteras mayores a cero.')
  validateSurtidoComposition(line)
  const gross = Math.round(line.presentation.precio * 100) * Number(line.quantity)
  if (!Number.isSafeInteger(gross) || gross > 9999999999) throw new Error('El importe de la línea excede el límite permitido.')
  const discount = cashCents(line.discount?.trim() || '0.00')
  if (discount === null) throw new Error('El descuento debe ser un importe no negativo con máximo dos decimales.')
  if (discount >= gross) throw new Error('El descuento debe ser menor al importe de la línea.')
  return gross - discount
}

export function saleTotals(lines: SaleLine[]) {
  if (!lines.length) throw new Error('Agrega al menos un producto.')
  const used = new Map<number, number>()
  const stocks = new Map<number, { stock: number; name: string }>()
  let total = 0
  let subtotal = 0
  for (const line of lines) {
    if (!/^\d+$/.test(line.quantity) || !Number.isSafeInteger(Number(line.quantity)) || Number(line.quantity) < 1)
      throw new Error('Ingresa cantidades enteras mayores a cero.')
    const quantity = Number(line.quantity)
    const consumption = line.product.id_surtido ? line.componentes ?? [] : [{ id_producto: line.product.id_producto, cantidad: quantity * line.presentation.unidades_equivalentes }]
    for (const c of consumption) {
      const product = line.product.id_surtido ? line.product.componentes?.find(p => p.id_producto === c.id_producto) : line.product
      if (!product) throw new Error('Producto no permitido en el surtido.')
      const units = (used.get(c.id_producto) ?? 0) + c.cantidad
      if (!Number.isSafeInteger(units)) throw new Error('La cantidad de unidades excede el límite permitido.')
      used.set(c.id_producto, units)
      const prior = stocks.get(c.id_producto)
      stocks.set(c.id_producto, { stock: Math.min(product.stock, prior?.stock ?? product.stock), name: product.nombre })
    }
    total += saleLineTotal(line)
    subtotal += Math.round(line.presentation.precio * 100) * quantity
    if (!Number.isSafeInteger(subtotal) || subtotal > 9999999999) throw new Error('El total excede el límite permitido.')
  }
  for (const [id, units] of used) if (units > stocks.get(id)!.stock) throw new Error(`Existencias insuficientes para ${stocks.get(id)!.name}.`)
  return total
}
