import { test } from 'node:test'
import assert from 'node:assert/strict'
import { createProduct } from '../src/features/products/api/productsApi.ts'
import { validateProduct } from '../src/features/products/schemas/productSchema.ts'
import { ApiError } from '../src/shared/api/ApiError.ts'

const valid = () => ({
  codigo_barra: '00123', nombre: 'Coca', idMarca: 1, stock_minimo: 0,
  presentaciones: [{ id_presentacion: 2, unidades_equivalentes: 6, precio: 25.50 }],
})
test('Producto valida cantidades enteras, precios y presentaciones repetidas', () => {
  assert.equal(validateProduct(valid()), undefined)
  assert.ok(validateProduct({ ...valid(), stock_minimo: -1 }))
  assert.ok(validateProduct({ ...valid(), presentaciones: [] }))
  assert.ok(validateProduct({ ...valid(), presentaciones: [valid().presentaciones[0], valid().presentaciones[0]] }))
  for (const change of [{ unidades_equivalentes: 1.5 }, { unidades_equivalentes: 0 }, { precio: NaN }, { precio: 1.001 }, { precio: 0 }])
    assert.ok(validateProduct({ ...valid(), presentaciones: [{ ...valid().presentaciones[0], ...change }] }))
})
test('Crear producto usa JSON autenticado sin enviar categoría, stock ni fecha', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Productos/crear')
    assert.equal(options.method, 'POST')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    assert.deepEqual(JSON.parse(options.body as string), valid())
    return Response.json({ id_producto: 3 }, { status: 201 })
  })
  assert.equal(await createProduct(valid(), 'token'), 3)
})
test('Imagen y presentaciones se envían con nombres indexados para ASP.NET', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Productos/crear/con-imagen')
    assert.ok(options.body instanceof FormData)
    assert.equal((options.headers as Record<string, string>)['Content-Type'], undefined)
    assert.equal(options.body.get('IdMarca'), '1')
    assert.equal(options.body.get('presentaciones[0].unidades_equivalentes'), '6')
    assert.equal(options.body.get('presentaciones[0].precio'), '25.5')
    assert.ok(options.body.get('Imagen') instanceof File)
    return Response.json({ id_producto: 3 })
  })
  await createProduct({ ...valid(), imagen: new File(['x'], 'a.jpg', { type: 'image/jpeg' }) }, 'token')
})
test('Duplicados, permisos y fallos de red no reintentan la creación', async t => {
  for (const status of [401, 403, 409]) {
    t.mock.method(globalThis, 'fetch', async () => new Response('secret', { status }))
    await assert.rejects(createProduct(valid(), 'token'), error =>
      error instanceof ApiError && error.status === status && !error.message.includes('secret'))
  }
  const mock = t.mock.method(globalThis, 'fetch', async () => { throw new TypeError('network') })
  await assert.rejects(createProduct(valid(), 'token'))
  assert.equal(mock.mock.callCount(), 1)
})
