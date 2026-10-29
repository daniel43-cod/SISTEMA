import test from 'node:test'
import assert from 'node:assert/strict'
import { createOperationId } from '../src/features/sales/utils/operationId.ts'

test('El identificador de venta funciona sin randomUUID al acceder por HTTP local', () => {
  const source = { getRandomValues: globalThis.crypto.getRandomValues.bind(globalThis.crypto) }
  const ids = Array.from({ length: 100 }, () => createOperationId(source))
  for (const id of ids) assert.match(id, /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/)
  assert.equal(new Set(ids).size, ids.length)
})

test('El identificador de venta usa randomUUID cuando está disponible', () => {
  const id = globalThis.crypto.randomUUID()
  assert.equal(createOperationId({ getRandomValues: globalThis.crypto.getRandomValues.bind(globalThis.crypto), randomUUID: () => id }), id)
})
import { cashCents, saleLineTotal, saleTotals } from '../src/features/sales/utils/saleValidation.ts'
import { filterSaleOptions, saleOptions } from '../src/features/sales/utils/saleSearch.ts'
const product = { id_producto: 1, nombre: 'Bebida', stock: 25, presentaciones: [] }
const unit = { id_producto_presentacion: 1, presentacion: 'Unidad', unidades_equivalentes: 1, precio: 3.35 }
const box = { id_producto_presentacion: 2, presentacion: 'Caja', unidades_equivalentes: 12, precio: 35 }
test('Venta suma en centavos y convierte efectivo con coma decimal', () => {
  assert.equal(saleTotals([{ product, presentation: unit, quantity: '3' }]), 1005)
  assert.equal(cashCents('10,05'), 1005)
  for (const value of ['-1', '1.001', '', 'NaN']) assert.equal(cashCents(value), null)
})
test('Venta acumula inventario compartido entre presentaciones', () => {
  assert.throws(() => saleTotals([{ product, presentation: box, quantity: '2' }, { product, presentation: unit, quantity: '2' }]), /Existencias/)
  assert.equal(saleTotals([{ product, presentation: box, quantity: '2' }, { product, presentation: unit, quantity: '1' }]), 7335)
})
test('Venta rechaza cantidades vacías, fraccionarias y negativas', () => {
  for (const quantity of ['', '0', '-1', '1.5']) assert.throws(() => saleTotals([{ product, presentation: unit, quantity }]))
  assert.throws(() => saleTotals([]))
})

test('La búsqueda ofrece cada presentación del mismo producto con sus IDs originales', () => {
  const products = [{ ...product, nombre: 'Coca Cola', presentaciones: [unit, box] }]
  const options = saleOptions(products)
  assert.deepEqual(options.map(option => option.label), ['Coca Cola · Unidad', 'Coca Cola · Caja'])
  const matches = filterSaleOptions(options, 'coca cola', [])
  assert.equal(matches.length, 2)
  assert.equal(matches[1].product.id_producto, product.id_producto)
  assert.equal(matches[1].presentation.id_producto_presentacion, box.id_producto_presentacion)
  assert.equal(matches[1].presentation.unidades_equivalentes, 12)
})

test('Buscar producto y presentación juntos filtra sin confundir caja y unidad', () => {
  const options = saleOptions([{ ...product, nombre: 'Coca Cola', presentaciones: [unit, box] }])
  assert.deepEqual(filterSaleOptions(options, 'COCA caja', []).map(option => option.presentation.id_producto_presentacion), [box.id_producto_presentacion])
  assert.equal(filterSaleOptions(options, '  ', []).length, 0)
})

test('Otra fila puede usar una presentación distinta sin repetir la ya seleccionada', () => {
  const options = saleOptions([{ ...product, nombre: 'Coca Cola', presentaciones: [unit, box] }])
  assert.deepEqual(filterSaleOptions(options, 'coca', [unit.id_producto_presentacion]).map(option => option.presentation.id_producto_presentacion), [box.id_producto_presentacion])
})

test('El código de barras ofrece las presentaciones del producto encontrado', () => {
  const options = saleOptions([
    { ...product, nombre: 'Coca Cola', presentaciones: [unit, box] },
    { ...product, id_producto: 2, nombre: 'Otro', presentaciones: [{ ...unit, id_producto_presentacion: 3 }] },
  ])
  assert.deepEqual(filterSaleOptions(options, '750000000001', [], 1).map(option => option.presentation.id_producto_presentacion), [1, 2])
})

test('El descuento se aplica una vez por línea y los totales usan centavos', () => {
  const presentation = { ...unit, precio: 120 }
  const line = { product, presentation, quantity: '2', discount: '19.00' }
  assert.equal(saleLineTotal(line), 22100)
  assert.equal(saleTotals([line]), 22100)
  assert.equal(saleTotals([{ product, presentation: unit, quantity: '3', discount: '0,05' }]), 1000)
})

test('Descuentos negativos, vacíos o de más de dos decimales bloquean el registro', () => {
  for (const discount of ['-1', '', '0.001', 'NaN', 'Infinity'])
    assert.throws(() => saleTotals([{ product, presentation: unit, quantity: '1', discount }]), /descuento/)
})

test('Un descuento no puede igualar o superar el importe de la línea', () => {
  for (const discount of ['3.35', '3.36'])
    assert.throws(() => saleTotals([{ product, presentation: unit, quantity: '1', discount }]), /descuento/)
})


import { lastSevenSaleDays } from '../src/features/sales/utils/saleDateRange.ts'
test('Últimos 7 días incluye hoy y seis días anteriores según Guatemala', () => {
  assert.deepEqual(lastSevenSaleDays(new Date('2026-10-29T18:00:00Z')), { desde: '2026-10-23', hasta: '2026-10-29' })
  assert.deepEqual(lastSevenSaleDays(new Date('2026-11-01T02:00:00Z')), { desde: '2026-10-25', hasta: '2026-10-31' })
})


import { saleHistoryQuery } from '../src/features/sales/utils/saleHistoryQuery.ts'
test('Ventas combina caja activa o fecha con el usuario seleccionado', () => {
  assert.equal(saleHistoryQuery(1), '/Venta/listar?pagina=1&tamanoPagina=50')
  assert.equal(saleHistoryQuery(1, undefined, 2), '/Venta/caja-activa/usuario/2?pagina=1&tamanoPagina=50')
  const range = { desde: '2026-10-23', hasta: '2026-10-23' }
  assert.equal(saleHistoryQuery(1, range), '/Venta/historial?pagina=1&tamanoPagina=50&desde=2026-10-23&hasta=2026-10-23')
  assert.equal(saleHistoryQuery(2, range, 3), '/Venta/historial?pagina=2&tamanoPagina=50&desde=2026-10-23&hasta=2026-10-23&idUsuario=3')
})


test('El tamaño seleccionado se envía en todos los modos del listado de ventas', () => {
  for (const size of [10, 25, 50]) {
    for (const url of [saleHistoryQuery(1, undefined, undefined, size), saleHistoryQuery(2, undefined, 2, size),
      saleHistoryQuery(1, { desde: '2026-10-23', hasta: '2026-10-23' }, 2, size)])
      assert.equal(new URL(url, 'https://example.test').searchParams.get('tamanoPagina'), String(size))
  }
})
