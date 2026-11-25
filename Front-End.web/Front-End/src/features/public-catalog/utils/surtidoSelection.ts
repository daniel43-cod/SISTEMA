import type { PublicSurtido, SurtidoCatalogSelection } from '../types/product.ts'
export type SurtidoCatalogInputs = Record<number, { cantidad: string; productos: Record<number, string> }>

export function surtidoCatalogSelections(product: PublicSurtido, inputs: SurtidoCatalogInputs): SurtidoCatalogSelection[] {
  const result: SurtidoCatalogSelection[] = []
  for (const [key, value] of Object.entries(inputs)) {
    const presentation = product.presentaciones.find(p => p.idProductoPresentacion === Number(key))
    if (!presentation) throw new Error('Presentación no disponible.')
    const quantity = Number(value.cantidad || '0')
    if (!/^\d*$/.test(value.cantidad) || !Number.isSafeInteger(quantity) || quantity < 0 || quantity > 999)
      throw new Error('La cantidad de presentaciones debe ser un entero entre 0 y 999.')
    const components = Object.entries(value.productos).map(([id, raw]) => {
      const component = product.productos.find(p => p.idProducto === Number(id))
      if (!component || (raw !== '' && (!/^\d+$/.test(raw) || !Number.isSafeInteger(Number(raw)) || Number(raw) > 2147483647)))
        throw new Error('Ingresa cantidades enteras no negativas de los productos permitidos.')
      const cantidad = Number(raw || '0')
      if (cantidad > 0 && !component.disponible) throw new Error(`${component.nombre} no está disponible.`)
      return { idProducto: component.idProducto, nombre: component.nombre, cantidad }
    }).filter(c => c.cantidad > 0)
    if (quantity === 0 && components.length === 0) continue
    if (!presentation.disponible) throw new Error('Esta presentación del surtido no está disponible.')
    const required = quantity * presentation.unidadesEquivalentes
    if (required <= 0 || required > 2147483647 || components.reduce((sum, c) => sum + c.cantidad, 0) !== required)
      throw new Error(`Completa exactamente ${required} unidades de ${presentation.nombre}.`)
    result.push({ idProductoPresentacion: presentation.idProductoPresentacion, cantidad: quantity, unidadesEquivalentes: presentation.unidadesEquivalentes, componentes: components })
  }
  if (!result.length) throw new Error('Selecciona una presentación y completa sus productos.')
  return result
}
