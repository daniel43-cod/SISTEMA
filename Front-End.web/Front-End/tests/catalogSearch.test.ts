import test from 'node:test'
import assert from 'node:assert/strict'
import { searchCatalogProducts, catalogSearchCards } from '../src/features/sales/utils/catalogSearch.ts'
import { saleOptions } from '../src/features/sales/utils/saleSearch.ts'
import type { SaleProduct } from '../src/features/sales/utils/saleValidation.ts'

const normal: SaleProduct = { id_producto: 1, nombre: 'Cola limón', imagen: '/uploads/productos/cola.webp', stock: 5,
  presentaciones: [{ id_producto_presentacion: 11, presentacion: 'Unidad', unidades_equivalentes: 1, precio: 4 },
    { id_producto_presentacion: 12, presentacion: 'Bandeja', unidades_equivalentes: 6, precio: 20 }] }
const mixed: SaleProduct = { id_producto: 0, id_surtido: 1, nombre: 'Cola surtida', imagen: '/uploads/productos/surtido.webp', stock: 10, bloqueo: null,
  componentes: [{ id_producto: 1, nombre: 'Limón', stock: 5 }, { id_producto: 2, nombre: 'Uva', stock: 0 }],
  presentaciones: [{ id_producto_presentacion: 13, presentacion: 'Bandeja', unidades_equivalentes: 6, precio: 20 }] }
const other: SaleProduct = { id_producto: 8, nombre: 'Jabón Blanca Nieves', stock: 30,
  presentaciones: [{ id_producto_presentacion: 20, presentacion: 'Docena', unidades_equivalentes: 12, precio: 24 }] }
const options = saleOptions([normal,mixed,other])
test('Búsqueda global encuentra otro producto sin depender de la marca abierta', () => {
  assert.deepEqual(searchCatalogProducts(options,'blanca').map(p => p.id_producto),[8])
})
test('Busca ignorando tildes, mayúsculas y espacios', () => {
  assert.deepEqual(searchCatalogProducts(options,'  COLA  limon ').map(p => p.id_producto),[1])
})
test('Busca nombre y presentación juntos y conserva todas las presentaciones de la tarjeta', () => {
  const result = searchCatalogProducts(options,'blanca docena')
  assert.equal(result[0].nombre,other.nombre)
  assert.equal(searchCatalogProducts(options,'limon bandeja')[0].presentaciones.length,2)
})
test('No repite la tarjeta por cada presentación y distingue producto de surtido con el mismo ID', () => {
  assert.equal(searchCatalogProducts(options,'cola').length,2)
})
test('Vacío, solo espacios y texto sin coincidencias no devuelven tarjetas', () => {
  for (const q of ['', '   ', 'inexistente']) assert.deepEqual(searchCatalogProducts(options,q),[])
})
test('Conserva imágenes, precios, IDs y disponibilidad al preparar las tarjetas', () => {
  const cards = catalogSearchCards([normal,mixed])
  assert.equal(cards.items[0].urlImagen,normal.imagen)
  assert.equal(cards.items[0].presentaciones[1].disponible,false)
  assert.equal(cards.items[0].presentaciones[0].idProductoPresentacion,11)
  assert.equal(cards.surtidos[0].urlImagen,mixed.imagen)
  assert.equal(cards.surtidos[0].productos[1].disponible,false)
  assert.equal(cards.surtidos[0].presentaciones[0].unidadesEquivalentes,6)
})
test('Una tarjeta de surtido bloqueado no se ofrece como disponible', () => {
  assert.equal(catalogSearchCards([{ ...mixed,bloqueo: 'Costos incompatibles' }]).surtidos[0].presentaciones[0].disponible,false)
})
test('Preparar y buscar no agrega líneas ni modifica el catálogo original', () => {
  const before = JSON.stringify(options)
  catalogSearchCards(searchCatalogProducts(options,'cola'))
  assert.equal(JSON.stringify(options),before)
  assert.equal(catalogSearchCards([other]).items[0].urlImagen,null)
})
