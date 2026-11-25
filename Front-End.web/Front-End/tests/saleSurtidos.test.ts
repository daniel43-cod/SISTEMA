import test from 'node:test'
import assert from 'node:assert/strict'
import { saleLineTotal, saleTotals, surtidoLineFromInputs } from '../src/features/sales/utils/saleValidation.ts'
import type { SaleProduct, SaleLine } from '../src/features/sales/utils/saleValidation.ts'
import { saleOptions, filterSaleOptions } from '../src/features/sales/utils/saleSearch.ts'
import { loadSaleProducts, loadSaleDetails } from '../src/features/sales/api/salesApi.ts'

const presentation = { id_producto_presentacion: 8, presentacion: 'Bandeja', unidades_equivalentes: 6, precio: 20 }
const group: SaleProduct = { id_producto: 0, id_surtido: 1, nombre: 'Cola surtida', stock: 12, bloqueo: null,
  componentes: [{ id_producto: 1, nombre: 'Limón', stock: 6 }, { id_producto: 2, nombre: 'Uva', stock: 6 }], presentaciones: [presentation] }
const make = (values: Record<number, string> = { 1: '3', 2: '3' }, quantity = '1', discount = '') => surtidoLineFromInputs(group, presentation, quantity, discount, values)

test('Surtido completa seis unidades y toma descuento vacío como cero', () => {
  const line = make()
  assert.deepEqual(line.componentes, [{ id_producto: 1, cantidad: 3 }, { id_producto: 2, cantidad: 3 }])
  assert.equal(saleTotals([line]), 2000)
})
test('Dos bandejas requieren doce unidades y no duplican descuento', () => {
  assert.equal(saleTotals([make({ 1: '6', 2: '6' }, '2', '2')]), 3800)
})
test('Permite toda la presentación de un solo sabor y omite componentes vacíos o cero', () => {
  assert.deepEqual(make({ 1: '6', 2: '' }).componentes, [{ id_producto: 1, cantidad: 6 }])
  assert.equal(make({ 1: '0', 2: '6' }).componentes!.length, 1)
})
for (const bad of ['1.5', '1.0', '-1', '1e0', 'NaN', 'Infinity', '+3', ' 3', '2147483648'])
  test(`No admite cantidad de componente ${bad}`, () => assert.throws(() => make({ 1: bad, 2: '3' })))
for (const quantity of ['', '0', '-1', '1.5', '1e1', '2147483647'])
  test(`No admite cantidad de presentaciones ${JSON.stringify(quantity)}`, () => assert.throws(() => make({ 1: '3', 2: '3' }, quantity)))
for (const values of [{} , { 1: '2', 2: '3' }, { 1: '4', 2: '3' }, { 1: '', 2: '' }, { 1: '3', 99: '3' }] as Record<number, string>[])
  test(`Composición inválida ${JSON.stringify(values)}`, () => assert.throws(() => make(values)))
test('Surtido bloqueado no puede agregarse aunque complete las unidades', () => {
  assert.throws(() => surtidoLineFromInputs({ ...group, bloqueo: 'Costos incompatibles' }, presentation, '1', '', { 1: '3', 2: '3' }), /Costos incompatibles/)
})
test('Inventario compartido con productos individuales no permite sobreventa', () => {
  const regular: SaleLine = { product: { id_producto: 1, nombre: 'Limón', stock: 6, presentaciones: [] },
    presentation: { ...presentation, unidades_equivalentes: 1, precio: 5, id_producto_presentacion: 9 }, quantity: '4' }
  assert.throws(() => saleTotals([make(), regular]), /Existencias insuficientes/)
  assert.equal(saleTotals([make(), { ...regular, quantity: '3' }]), 3500)
})
test('Dos surtidos no pueden consumir las mismas últimas unidades', () => {
  const other = make(); other.componentes = [{ id_producto: 1, cantidad: 6 }]
  assert.throws(() => saleTotals([make(), other]), /Existencias insuficientes/)
})
test('No permite componentes repetidos ni fraccionarios en líneas manipuladas', () => {
  const line = make(); line.componentes = [{ id_producto: 1, cantidad: 3 }, { id_producto: 1, cantidad: 3 }]
  assert.throws(() => saleLineTotal(line))
  line.componentes = [{ id_producto: 1, cantidad: 2.5 }, { id_producto: 2, cantidad: 3.5 }]
  assert.throws(() => saleLineTotal(line))
})
test('Cambiar cantidad obliga a completar otra vez la composición', () => {
  assert.throws(() => saleTotals([{ ...make(), quantity: '2' }]), /exactamente 12/)
})
for (const discount of ['-1', '1.001', '20', '21'])
  test(`Descuento inválido ${discount} bloquea agregar`, () => assert.throws(() => make(undefined, '1', discount)))
test('Buscador incluye surtido por su nombre y presentación', () => {
  assert.equal(filterSaleOptions(saleOptions([group]), 'surtida bandeja', []).length, 1)
})
test('API carga productos y surtidos separados sin confundir sus identificadores', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string) => Response.json(url.includes('/surtidos?') ? [group] : []))
  assert.deepEqual(await loadSaleProducts('token'), [group])
})
test('Detalle de surtido conserva composición, nombre e identificador sin producto ficticio', async t => {
  const detail = { id_producto: null, id_surtido: 1, nombre_producto: 'Cola surtida', descripcion_resentacion: 'Bandeja', cantidad: 1, precio: 20, descuento: 0,
    componentes: [{ id_producto: 1, nombre: 'Limón', cantidad: 6 }] }
  t.mock.method(globalThis, 'fetch', async () => Response.json([detail]))
  assert.deepEqual(await loadSaleDetails(1, 'token'), [detail])
})
