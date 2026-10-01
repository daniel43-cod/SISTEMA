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
