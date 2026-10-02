import { test } from 'node:test'
import assert from 'node:assert/strict'
import { listBrands } from '../src/features/brands/api/brandsApi.ts'
import { ApiError } from '../src/shared/api/ApiError.ts'

test('Listado de marcas usa GET autenticado y admite lista vacía', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Marcas')
    assert.equal(options.method, 'GET')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    return Response.json([])
  })
  assert.deepEqual(await listBrands('token'), [])
})
test('Listado valida marcas, categorías e identificadores duplicados', async t => {
  const row = { idMarca: 1, nombre: 'Marca', idCategoria: 2, estado: true }
  t.mock.method(globalThis, 'fetch', async () => Response.json([row]))
  assert.deepEqual(await listBrands('token'), [row])
  for (const data of [{}, [row, row], [{ ...row, idCategoria: 0 }], [{ ...row, estado: false }]]) {
    t.mock.method(globalThis, 'fetch', async () => Response.json(data))
    await assert.rejects(listBrands('token'), ApiError)
  }
})
test('Listado conserva errores de sesión y permisos sin exponer detalles', async t => {
  for (const status of [401, 403, 500]) {
    t.mock.method(globalThis, 'fetch', async () => new Response('secret', { status }))
    await assert.rejects(listBrands('token'), error =>
      error instanceof ApiError && error.status === status && !error.message.includes('secret'))
  }
})

test('Editar marca envía PUT con nombre y categoría, sin estado ni ID en el cuerpo', async t => {
  const { updateBrand } = await import('../src/features/brands/api/brandsApi.ts')
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Marcas/3')
    assert.equal(options.method, 'PUT')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    assert.deepEqual(JSON.parse(options.body as string), { nombre: 'Marca', idCategoria: 2 })
    return Response.json({ idMarca: 3, nombre: 'Marca', idCategoria: 2, estado: true })
  })
  assert.equal((await updateBrand(3, { nombre: ' Marca ', idCategoria: 2 }, 'token')).idMarca, 3)
})
test('Editar rechaza ID inválido y conserva errores de permisos, duplicado y registro inexistente', async t => {
  const { updateBrand } = await import('../src/features/brands/api/brandsApi.ts')
  const mock = t.mock.method(globalThis, 'fetch', async () => new Response(null, { status: 404 }))
  await assert.rejects(updateBrand(0, { nombre: 'Marca', idCategoria: 2 }, 'token'))
  assert.equal(mock.mock.callCount(), 0)
  for (const status of [401, 403, 404, 409]) {
    t.mock.method(globalThis, 'fetch', async () => new Response('secret', { status }))
    await assert.rejects(updateBrand(3, { nombre: 'Marca', idCategoria: 2 }, 'token'),
      error => error instanceof ApiError && error.status === status && !error.message.includes('secret'))
  }
})
