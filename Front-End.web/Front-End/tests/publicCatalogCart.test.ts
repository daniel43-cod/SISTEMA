import assert from 'node:assert/strict'
import test from 'node:test'
import { cartTotalCents, MAX_QUANTITY, parseCart, setCartQuantity } from '../src/features/public-catalog/utils/cart.ts'
const unit = { idProducto: 1, idProductoPresentacion: 10, producto: 'Agua', presentacion: 'Unidad', precio: 0.1 }
test('Separates presentations of the same product and updates without duplicates', () => {
  let cart = setCartQuantity([], unit, 2)
  cart = setCartQuantity(cart, { ...unit, idProductoPresentacion: 11, presentacion: 'Caja', precio: 12 }, 1)
  cart = setCartQuantity(cart, unit, 3)
  assert.equal(cart.length, 2)
  assert.equal(cart[0].cantidad, 3)
  assert.equal(cart[1].cantidad, 1)
  assert.equal(cartTotalCents(cart), 1230)
})
test('Zero removes a line and invalid quantities do not modify the cart', () => {
  const cart = setCartQuantity([], unit, 1)
  for (const quantity of [-1, 1.5, NaN, MAX_QUANTITY + 1]) assert.deepEqual(setCartQuantity(cart, unit, quantity), cart)
  assert.deepEqual(setCartQuantity(cart, unit, 0), [])
})
test('Restores valid cart and rejects corrupted or duplicated stored entries', () => {
  const cart = setCartQuantity([], unit, 3)
  assert.deepEqual(parseCart(JSON.stringify(cart)), cart)
  for (const value of ['broken', '{}', JSON.stringify([...cart, ...cart]), JSON.stringify([{ ...cart[0], cantidad: -1 }])])
    assert.deepEqual(parseCart(value), [])
})
test('Money totals avoid binary floating-point fractions', () => {
  const cart = setCartQuantity([], unit, 3)
  assert.equal(cartTotalCents(cart), 30)
})
