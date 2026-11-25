import type { CartItem } from '../types/product.ts'

export const CART_KEY = 'sistema.public-catalog.cart.v1'
export const MAX_QUANTITY = 999
const maxLines = 200

function validComposition(row: Record<string, unknown>): boolean {
  if (row.idSurtido === undefined) return row.componentes === undefined || (Array.isArray(row.componentes) && row.componentes.length === 0)
  if (row.idProducto !== 0 || !Number.isSafeInteger(row.idSurtido) || Number(row.idSurtido) <= 0 ||
      !Number.isSafeInteger(row.unidadesEquivalentes) || Number(row.unidadesEquivalentes) <= 0 ||
      !Array.isArray(row.componentes) || row.componentes.length < 1) return false
  const ids = new Set<number>()
  let total = 0
  for (const item of row.componentes) {
    if (!item || typeof item !== 'object' || !Number.isSafeInteger(item.idProducto) || item.idProducto <= 0 || ids.has(item.idProducto) ||
        typeof item.nombre !== 'string' || !item.nombre.trim() || item.nombre.length > 500 || !Number.isSafeInteger(item.cantidad) || item.cantidad < 1 || item.cantidad > 2147483647) return false
    ids.add(item.idProducto); total += item.cantidad
  }
  return total === Number(row.cantidad) * Number(row.unidadesEquivalentes) && total <= 2147483647
}

export function parseCart(value: string | null): CartItem[] {
  try {
    const rows: unknown = JSON.parse(value ?? '[]')
    if (!Array.isArray(rows) || rows.length > maxLines) return []
    const ids = new Set<number>()
    const valid = rows.every((value: unknown) => {
      if (!value || typeof value !== 'object') return false
      const row = value as Record<string, unknown>
      if (typeof row.idProducto !== 'number' || !Number.isSafeInteger(row.idProducto) || (row.idProducto <= 0 && !(row.idProducto === 0 && row.idSurtido !== undefined)) ||
          typeof row.idProductoPresentacion !== 'number' || !Number.isSafeInteger(row.idProductoPresentacion) ||
          row.idProductoPresentacion <= 0 || ids.has(row.idProductoPresentacion) ||
          typeof row.producto !== 'string' || !row.producto.trim() || row.producto.length > 500 ||
          typeof row.presentacion !== 'string' || !row.presentacion.trim() || row.presentacion.length > 500 ||
          typeof row.precio !== 'number' || !Number.isFinite(row.precio) || row.precio < 0 ||
          typeof row.cantidad !== 'number' || !Number.isSafeInteger(row.cantidad) ||
          row.cantidad < 1 || row.cantidad > MAX_QUANTITY || !validComposition(row)) return false
      ids.add(row.idProductoPresentacion)
      return true
    })
    return valid ? rows.map(row => ({
      idProducto: row.idProducto, idProductoPresentacion: row.idProductoPresentacion,
      producto: row.producto, presentacion: row.presentacion, precio: row.precio, cantidad: row.cantidad,
      ...(row.idSurtido !== undefined ? { idSurtido: row.idSurtido, unidadesEquivalentes: row.unidadesEquivalentes,
        componentes: row.componentes.map((c: { idProducto: number; nombre: string; cantidad: number }) => ({ idProducto: c.idProducto, nombre: c.nombre, cantidad: c.cantidad })) } : {}),
    })) : []
  } catch { return [] }
}

export function setCartQuantity(items: CartItem[], item: Omit<CartItem, 'cantidad'>, cantidad: number): CartItem[] {
  if (!Number.isSafeInteger(cantidad) || cantidad < 0 || cantidad > MAX_QUANTITY) return items
  if (cantidad === 0) return items.filter(row => row.idProductoPresentacion !== item.idProductoPresentacion)
  if (!validComposition({ ...item, cantidad })) return items
  const index = items.findIndex(row => row.idProductoPresentacion === item.idProductoPresentacion)
  if (index < 0) return items.length >= maxLines ? items : [...items, { ...item, cantidad }]
  return items.map((row, i) => i === index ? { ...item, cantidad } : row)
}

export function cartTotalCents(items: CartItem[]): number {
  return items.reduce((total, item) => total + Math.round(item.precio * 100) * item.cantidad, 0)
}
