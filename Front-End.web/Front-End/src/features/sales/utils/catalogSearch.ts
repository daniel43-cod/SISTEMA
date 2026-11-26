import type { SaleOption } from './saleSearch.ts'
import type { SaleProduct } from './saleValidation.ts'
import type { PublicProduct, PublicSurtido } from '../../public-catalog/types/product.ts'

const normalize = (value: string) => value.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLocaleLowerCase().trim()
export function searchCatalogProducts(options: SaleOption[], query: string): SaleProduct[] {
  const words = normalize(query).split(/\s+/).filter(Boolean)
  if (!words.length) return []
  const found = new Map<string, SaleProduct>()
  for (const option of options) {
    if (words.every(word => normalize(option.label).includes(word)))
      found.set(option.product.id_surtido ? `s-${option.product.id_surtido}` : `p-${option.product.id_producto}`, option.product)
  }
  return [...found.values()]
}
export function catalogSearchCards(products: SaleProduct[]): { items: PublicProduct[]; surtidos: PublicSurtido[] } {
  return {
    items: products.filter(p => !p.id_surtido).map(p => ({ idProducto: p.id_producto, nombre: p.nombre, urlImagen: p.imagen ?? null,
      presentaciones: p.presentaciones.map(pr => ({ idProductoPresentacion: pr.id_producto_presentacion, nombre: pr.presentacion, precio: pr.precio, disponible: p.stock >= pr.unidades_equivalentes })) })),
    surtidos: products.filter(p => p.id_surtido).map(p => ({ idSurtido: p.id_surtido!, nombre: p.nombre, urlImagen: p.imagen ?? null,
      productos: (p.componentes ?? []).map(c => ({ idProducto: c.id_producto, nombre: c.nombre, disponible: c.stock > 0 })),
      presentaciones: p.presentaciones.map(pr => ({ idProductoPresentacion: pr.id_producto_presentacion, nombre: pr.presentacion, precio: pr.precio,
        unidadesEquivalentes: pr.unidades_equivalentes, disponible: !p.bloqueo })) })),
  }
}
