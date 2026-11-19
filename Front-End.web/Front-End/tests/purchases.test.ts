import { getPurchaseSummary } from '../src/features/purchases/api/purchasesApi.ts'
import test from 'node:test'
import assert from 'node:assert/strict'
import { amountCents, buildPurchase } from '../src/features/purchases/utils/purchaseValidation.ts'
const line = { idProducto: 1, nombre: 'Coca Cola', cantidad: '60', total: '920' }
test('Purchases send base units and line total without multiplying by quantity', () => {
  const result = buildPurchase('2', ' Entrada ', '200', [line])
  assert.deepEqual(result.detalle_compra, [{ id_producto: 1, cantidad: 60, precio: 920 }])
  assert.equal(result.monto_pagado, 200)
  assert.equal(result.observacion, 'Entrada')
})
test('Money accepts cents and rejects extra decimals or negative values', () => {
  assert.equal(amountCents('920,50'), 92050)
  for (const value of ['1.001', '-1', '', 'NaN', '100000000']) assert.equal(amountCents(value), null)
})
test('Invalid quantities, duplicate lines and overpayments are rejected', () => {
  assert.throws(() => buildPurchase('2', '', '921', [line]))
  assert.throws(() => buildPurchase('2', '', '0', [line, line]))
  assert.throws(() => buildPurchase('2', '', '0', [{ ...line, cantidad: '1.5' }]))
  assert.throws(() => buildPurchase('', '', '0', [line]))
  assert.throws(() => buildPurchase('2', '', '0', []))
})

test('Purchase summary sends search and payment state and uses backend totals', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string) => {
    const params = new URL(url, 'https://test.invalid').searchParams
    assert.equal(params.get('buscar'), 'Proveedor')
    assert.equal(params.get('estadoPago'), 'pendientes')
    assert.equal(params.get('pagina'), '2')
    return Response.json({ totalRegistros: 21, cantidadPendientes: 21, cantidadPagadas: 2,
      totalComprado: 590, totalPorPagar: 490, totalPagado: 100, hayMas: false, items: [] })
  })
  const result = await getPurchaseSummary('token', 2, 'Proveedor', 'pendientes')
  assert.equal(result.totalComprado, 590)
  assert.equal(result.totalPagado, 100)
  assert.equal(result.totalPorPagar, 490)
})
test('Purchase summary rejects invalid totals instead of displaying zero', async t => {
  t.mock.method(globalThis, 'fetch', async () => Response.json({ items: [] }))
  await assert.rejects(getPurchaseSummary('token', 1, '', ''), /respuesta de compras/)
})