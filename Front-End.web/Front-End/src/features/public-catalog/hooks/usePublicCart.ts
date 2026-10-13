import { useRef, useState } from 'react'
import type { CartItem } from '../types/product'
import { CART_KEY, cartTotalCents, parseCart, setCartQuantity } from '../utils/cart'

export function usePublicCart() {
  const [items, setItems] = useState<CartItem[]>(() => {
    try { return parseCart(localStorage.getItem(CART_KEY)) } catch { return [] }
  })
  const currentItems = useRef(items)
  const [storageError, setStorageError] = useState('')
  function save(next: CartItem[]) {
    currentItems.current = next
    setItems(next)
    try {
      localStorage.setItem(CART_KEY, JSON.stringify(next))
      setStorageError('')
    } catch { setStorageError('El carrito funciona en esta página, pero no pudo guardarse para tu próxima visita.') }
  }
  function setQuantity(item: Omit<CartItem, 'cantidad'>, cantidad: number) {
    save(setCartQuantity(currentItems.current, item, cantidad))
  }
  return { items, setQuantity, clear: () => save([]), storageError,
    count: items.reduce((sum, row) => sum + row.cantidad, 0), total: cartTotalCents(items) / 100 }
}

