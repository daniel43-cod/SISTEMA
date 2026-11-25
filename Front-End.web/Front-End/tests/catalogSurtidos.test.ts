import test from 'node:test'
import assert from 'node:assert/strict'
import { surtidoCatalogSelections } from '../src/features/public-catalog/utils/surtidoSelection.ts'
import { parseCart, setCartQuantity } from '../src/features/public-catalog/utils/cart.ts'
import { listPublicSurtidos } from '../src/features/public-catalog/api/publicProductsApi.ts'
import type { PublicSurtido, CartItem } from '../src/features/public-catalog/types/product.ts'

const product: PublicSurtido = { idSurtido: 1, nombre: 'Cola surtida', urlImagen: null,
  productos: [{ idProducto: 1, nombre: 'Limón', disponible: true }, { idProducto: 2, nombre: 'Uva', disponible: true }],
  presentaciones: [{ idProductoPresentacion: 10, nombre: 'Bandeja', precio: 20, disponible: true, unidadesEquivalentes: 6 }] }
const selection = () => surtidoCatalogSelections(product, { 10: { cantidad: '1', productos: { 1: '3', 2: '3' } } })
const cart = (): CartItem => ({ idProducto: 0, idSurtido: 1, idProductoPresentacion: 10, producto: 'Cola surtida', presentacion: 'Bandeja',
  precio: 20, cantidad: 1, unidadesEquivalentes: 6, componentes: selection()[0].componentes })

test('Catálogo prepara presentación y composición exacta sin añadir nada antes del botón', () => {
  assert.deepEqual(selection()[0].componentes, [{ idProducto: 1, nombre: 'Limón', cantidad: 3 }, { idProducto: 2, nombre: 'Uva', cantidad: 3 }])
})
test('Dos presentaciones exigen doce unidades', () => {
  assert.equal(surtidoCatalogSelections(product, { 10: { cantidad: '2', productos: { 1: '6', 2: '6' } } })[0].cantidad, 2)
})
for (const value of ['-1', '1.5', '1e1', 'NaN', '2147483648'])
  test('Catálogo rechaza cantidad de producto ' + value, () => assert.throws(() => surtidoCatalogSelections(product, { 10: { cantidad: '1', productos: { 1: value, 2: '3' } } })))
for (const value of ['-1', '1.5', '1000', '1e1'])
  test('Catálogo rechaza cantidad de presentaciones ' + value, () => assert.throws(() => surtidoCatalogSelections(product, { 10: { cantidad: value, productos: { 1: '3', 2: '3' } } })))
test('Rechaza cantidades incompletas o excedidas, componentes ajenos y presentaciones ajenas', () => {
  for (const first of ['2', '4']) assert.throws(() => surtidoCatalogSelections(product, { 10: { cantidad: '1', productos: { 1: first, 2: '3' } } }))
  assert.throws(() => surtidoCatalogSelections(product, { 10: { cantidad: '1', productos: { 99: '6' } } }))
  assert.throws(() => surtidoCatalogSelections(product, { 99: { cantidad: '1', productos: { 1: '6' } } }))
})
test('No añade productos ni presentaciones marcados como no disponibles', () => {
  assert.throws(() => surtidoCatalogSelections({ ...product, productos: product.productos.map(p => ({ ...p, disponible: false })) }, { 10: { cantidad: '1', productos: { 1: '6' } } }))
  assert.throws(() => surtidoCatalogSelections({ ...product, presentaciones: product.presentaciones.map(p => ({ ...p, disponible: false })) }, { 10: { cantidad: '1', productos: { 1: '6' } } }))
})
test('Carrito público conserva composición al restaurarlo desde almacenamiento', () => {
  assert.deepEqual(parseCart(JSON.stringify([cart()])), [cart()])
})
test('Carrito rechaza composición incompleta, fraccionaria o duplicada al restaurar', () => {
  const row = cart()
  for (const componentes of [[{ idProducto: 1, nombre: 'Limón', cantidad: 5 }],
    [{ idProducto: 1, nombre: 'Limón', cantidad: 2.5 }, { idProducto: 2, nombre: 'Uva', cantidad: 3.5 }],
    [{ idProducto: 1, nombre: 'Limón', cantidad: 3 }, { idProducto: 1, nombre: 'Limón', cantidad: 3 }]])
    assert.deepEqual(parseCart(JSON.stringify([{ ...row, componentes }])), [])
})
test('Cambiar cantidad del carrito exige una composición nueva completa', () => {
  const row = cart(); const original = [row]
  assert.equal(setCartQuantity(original, row, 2), original)
  assert.deepEqual(setCartQuantity(original, row, 0), [])
  assert.equal(setCartQuantity(original, { ...row, componentes: row.componentes!.map(c => ({ ...c, cantidad: 6 })) }, 2)[0].cantidad, 2)
})
test('API pública consulta surtidos por marca con paginación y sin autenticación interna', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/CatalogoPublico/marcas/1/surtidos?pagina=1&tamanoPagina=24')
    assert.equal((options.headers as Record<string, string>).Authorization, undefined)
    return Response.json({ pagina: 1, tamanoPagina: 24, hayMas: false, surtidos: [product] })
  })
  assert.deepEqual((await listPublicSurtidos(1, 1)).surtidos, [product])
})
test('API pública rechaza unidades o productos malformados', async t => {
  t.mock.method(globalThis, 'fetch', async () => Response.json({ pagina: 1, tamanoPagina: 24, hayMas: false,
    surtidos: [{ ...product, presentaciones: [{ ...product.presentaciones[0], unidadesEquivalentes: -1 }] }] }))
  await assert.rejects(listPublicSurtidos(1, 1))
})
