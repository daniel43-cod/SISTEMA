import type { SaleLine, SalePresentation, SaleProduct } from './saleValidation.ts'

export type SaleOption = { product: SaleProduct; presentation: SalePresentation; label: string }
export type SaleDraft = { key: number; text: string; barcode?: string; selection: SaleLine | null }

export function saleOptions(products: SaleProduct[]): SaleOption[] {
  return products.flatMap(product => product.presentaciones.map(presentation => ({
    product, presentation, label: `${product.nombre} · ${presentation.presentacion}`,
  })))
}

export function filterSaleOptions(options: SaleOption[], query: string, usedIds: number[], barcodeProduct?: number): SaleOption[] {
  const words = query.trim().toLocaleLowerCase().split(/\s+/).filter(Boolean)
  if (!words.length && barcodeProduct === undefined) return []
  return options.filter(option => !usedIds.includes(option.presentation.id_producto_presentacion) &&
    (barcodeProduct !== undefined ? option.product.id_producto === barcodeProduct : words.every(word => option.label.toLocaleLowerCase().includes(word))))
}
