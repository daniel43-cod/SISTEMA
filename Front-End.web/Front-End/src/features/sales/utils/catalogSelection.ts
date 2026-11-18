import type { SaleOption } from './saleSearch.ts'
import { saleTotals } from './saleValidation.ts'
import type { SaleLine } from './saleValidation.ts'

export function catalogSaleLine(productId: number, presentationId: number, quantity: string,
  options: SaleOption[], existing: SaleLine[]): SaleLine {
  if (existing.some(line => line.presentation.id_producto_presentacion === presentationId))
    throw new Error('Esta presentación ya está agregada. Editala desde Nueva venta.')
  const option = options.find(item => item.product.id_producto === productId &&
    item.presentation.id_producto_presentacion === presentationId)
  if (!option) throw new Error('Esta presentación no está disponible para la venta. Actualiza el catálogo de Ventas.')
  const selection: SaleLine = { product: option.product, presentation: option.presentation, quantity, discount: '0.00' }
  saleTotals([...existing, selection])
  return selection
}
export function catalogSaleLines(productId: number,
  selections: { presentationId: number; quantity: string }[], options: SaleOption[], existing: SaleLine[]): SaleLine[] {
  if (!selections.length) throw new Error('Selecciona una cantidad en al menos una presentación.')
  const added: SaleLine[] = []
  for (const selection of selections)
    added.push(catalogSaleLine(productId, selection.presentationId, selection.quantity, options, [...existing, ...added]))
  return added
}