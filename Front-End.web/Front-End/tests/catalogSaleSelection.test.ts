import { test } from 'node:test'
import assert from 'node:assert/strict'
import { catalogSaleLine, catalogSaleLines } from '../src/features/sales/utils/catalogSelection.ts'
import { saleOptions } from '../src/features/sales/utils/saleSearch.ts'

const product = { id_producto: 1, nombre: 'Producto', stock: 10, presentaciones: [
  { id_producto_presentacion: 1, presentacion: 'Unidad', unidades_equivalentes: 1, precio: 5 },
  { id_producto_presentacion: 2, presentacion: 'Paquete', unidades_equivalentes: 6, precio: 25 },
] }
const options = saleOptions([product])
test('Catalog selection uses sale price, adds no discount and preserves the existing sale', () => {
  const existing = [{ product, presentation: product.presentaciones[0], quantity: '2', discount: '1.00' }]
  const result = catalogSaleLine(1, 2, '1', options, existing)
  assert.equal(result.presentation.precio, 25)
  assert.equal(result.discount, '0.00')
  assert.equal(result.quantity, '1')
  assert.equal(existing.length, 1)
  assert.equal(existing[0].discount, '1.00')
})
test('Catalog rejects duplicates without changing existing quantities or discounts', () => {
  const existing = [{ product, presentation: product.presentaciones[0], quantity: '2', discount: '1.00' }]
  assert.throws(() => catalogSaleLine(1, 1, '3', options, existing), /ya está agregada/)
  assert.equal(existing[0].quantity, '2')
  assert.equal(existing[0].discount, '1.00')
})
test('Catalog validates combined stock across presentations, identity and quantity', () => {
  const existing = [{ product, presentation: product.presentaciones[0], quantity: '5', discount: '0.00' }]
  assert.throws(() => catalogSaleLine(1, 2, '1', options, existing), /Existencias insuficientes/)
  assert.throws(() => catalogSaleLine(9, 1, '1', options, []), /no está disponible/)
  assert.throws(() => catalogSaleLine(1, 99, '1', options, []), /no está disponible/)
  for (const quantity of ['0', '-1', '1.5', '', 'abc'])
    assert.throws(() => catalogSaleLine(1, 1, quantity, options, []), /cantidades enteras/)
})
test('One product card adds its selected presentations together', () => {
  const added = catalogSaleLines(1, [
    { presentationId: 1, quantity: '2' }, { presentationId: 2, quantity: '1' },
  ], options, [])
  assert.equal(added.length, 2)
  assert.deepEqual(added.map(line => line.quantity), ['2', '1'])
  assert.ok(added.every(line => line.discount === '0.00'))
})
test('A failed card selection never partially changes the existing sale', () => {
  const existing = [{ product, presentation: product.presentaciones[0], quantity: '2', discount: '1.00' }]
  const before = structuredClone(existing)
  assert.throws(() => catalogSaleLines(1, [
    { presentationId: 2, quantity: '1' }, { presentationId: 99, quantity: '1' },
  ], options, existing), /no está disponible/)
  assert.deepEqual(existing, before)
  assert.throws(() => catalogSaleLines(1, [
    { presentationId: 1, quantity: '5' }, { presentationId: 2, quantity: '1' },
  ], options, []), /Existencias insuficientes/)
  assert.throws(() => catalogSaleLines(1, [], options, []), /al menos una/)
})