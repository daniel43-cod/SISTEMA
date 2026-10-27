import { useState } from 'react'
import { PublicCheckoutForm } from './PublicCheckoutForm'
import type { CartItem } from '../types/product'
import { MAX_QUANTITY } from '../utils/cart'

const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
export function PublicCart({ items, total, count, storageError, setQuantity, clear, onBusy }: {
  items: CartItem[]
  total: number
  count: number
  storageError: string
  setQuantity: (item: Omit<CartItem, 'cantidad'>, cantidad: number) => void
  onBusy?: (value: boolean) => void
  clear: () => void
}) {
  const [checkout, setCheckout] = useState(false)
  const [success, setSuccess] = useState('')
  return <section className="public-cart" aria-labelledby="public-cart-title">
    <h2 id="public-cart-title">Tu carrito <span>({count})</span></h2>
    {success && <p role="status">{success}</p>}
    {storageError && <p role="status">{storageError}</p>}
    {!items.length ? <p>Agrega presentaciones con el botón + de cada producto.</p> : checkout ? <PublicCheckoutForm items={items} total={total} onBack={() => setCheckout(false)} onBusy={value => onBusy?.(value)}
      onSuccess={confirmedTotal => { setSuccess('Pedido recibido, pendiente de aprobación. Total: ' + money.format(confirmedTotal)); setCheckout(false); clear() }} /> : <>
      <ul className="public-cart-items">
        {items.map(item => <li key={item.idProductoPresentacion}>
          <div><strong>{item.producto}</strong><span>{item.presentacion} · {money.format(item.precio)} c/u</span></div>
          <div className="public-quantity" role="group" aria-label={`Cantidad de ${item.producto}, ${item.presentacion}`}>
            <button type="button" aria-label={`Reducir ${item.producto}, ${item.presentacion}`}
              onClick={() => setQuantity(item, item.cantidad - 1)}>−</button>
            <output>{item.cantidad}</output>
            <button type="button" aria-label={`Aumentar ${item.producto}, ${item.presentacion}`}
              disabled={item.cantidad >= MAX_QUANTITY} onClick={() => setQuantity(item, item.cantidad + 1)}>+</button>
          </div>
          <span>{money.format(Math.round(item.precio * 100) * item.cantidad / 100)}</span>
          <button type="button" className="public-cart-remove" onClick={() => setQuantity(item, 0)}
            aria-label={`Eliminar ${item.producto}, ${item.presentacion}`}>Eliminar</button>
        </li>)}
      </ul>
      <p className="public-cart-total">Total estimado <strong>{money.format(total)}</strong></p>
      <button type="button" className="public-checkout-confirm" onClick={() => { setSuccess(''); setCheckout(true) }}>Confirmar pedido</button>
      <button type="button" className="public-catalog-back" onClick={clear}>Vaciar carrito</button>
      <p className="public-cart-note">El carrito no reserva inventario. Los precios y existencias se verificarán al confirmar el pedido.</p>
    </>}
  </section>
}
