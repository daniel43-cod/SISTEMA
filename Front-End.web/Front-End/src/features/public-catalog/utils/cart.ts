import type { CartItem } from '../types/product.ts'

export const CART_KEY = 'sistema.public-catalog.cart.v1'
export const MAX_QUANTITY = 999
const maxLines = 200

export function parseCart(value: string | null): CartItem[] {
  try {
    const rows: unknown = JSON.parse(value ?? '[]')
    if (!Array.isArray(rows) || rows.length > maxLines) return []
    const ids = new Set<number>()
    const valid = rows.every((value: unknown) => {
      if (!value || typeof value !== 'object') return false
      const row = value as Record<string, unknown>
      if (typeof row.idProducto !== 'number' || !Number.isSafeInteger(row.idProducto) || row.idProducto <= 0 ||
          typeof row.idProductoPresentacion !== 'number' || !Number.isSafeInteger(row.idProductoPresentacion) ||
          row.idProductoPresentacion <= 0 || ids.has(row.idProductoPresentacion) ||
          typeof row.producto !== 'string' || !row.producto.trim() || row.producto.length > 500 ||
          typeof row.presentacion !== 'string' || !row.presentacion.trim() || row.presentacion.length > 500 ||
          typeof row.precio !== 'number' || !Number.isFinite(row.precio) || row.precio < 0 ||
          typeof row.cantidad !== 'number' || !Number.isSafeInteger(row.cantidad) ||
          row.cantidad < 1 || row.cantidad > MAX_QUANTITY) return false
      ids.add(row.idProductoPresentacion)
      return true
    })
    return valid ? rows.map(row => ({
      idProducto: row.idProducto, idProductoPresentacion: row.idProductoPresentacion,
      producto: row.producto, presentacion: row.presentacion, precio: row.precio, cantidad: row.cantidad,
    })) : []
  } catch { return [] }
}

export function setCartQuantity(items: CartItem[], item: Omit<CartItem, 'cantidad'>, cantidad: number): CartItem[] {
  if (!Number.isSafeInteger(cantidad) || cantidad < 0 || cantidad > MAX_QUANTITY) return items
  if (cantidad === 0) return items.filter(row => row.idProductoPresentacion !== item.idProductoPresentacion)
  const index = items.findIndex(row => row.idProductoPresentacion === item.idProductoPresentacion)
  if (index < 0) return items.length >= maxLines ? items : [...items, { ...item, cantidad }]
  return items.map((row, i) => i === index ? { ...item, cantidad } : row)
}

export function cartTotalCents(items: CartItem[]): number {
  return items.reduce((total, item) => total + Math.round(item.precio * 100) * item.cantidad, 0)
}
