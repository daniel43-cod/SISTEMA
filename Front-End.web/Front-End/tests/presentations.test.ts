import { test } from 'node:test'
import assert from 'node:assert/strict'
import { validateDescription, parsePresentation } from '../src/features/presentations/schemas/presentationSchema.ts'
import { createPresentation, listPresentations, updatePresentation } from '../src/features/presentations/api/presentationsApi.ts'
import { ApiError } from '../src/shared/api/ApiError.ts'

test('Descripción obligatoria y hasta 100 caracteres; respuesta válida', () => {
  assert.ok(validateDescription('   '))
  assert.ok(validateDescription('x'.repeat(101)))
  assert.equal(validateDescription('Caja'), undefined)
  assert.equal(validateDescription('x'.repeat(100)), undefined)
  assert.equal(parsePresentation({ idPresentacion: 1, descripcion: 'Caja', estado: true }).idPresentacion, 1)
  assert.throws(() => parsePresentation({ idPresentacion: '1', descripcion: 'Caja', estado: true }))
  assert.throws(() => parsePresentation(null))
})
test('Crear envía POST con token y descripción limpia, sin estado ni IDs', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Presentaciones')
    assert.equal(options.method, 'POST')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer test-token')
    assert.deepEqual(JSON.parse(options.body as string), { descripcion: 'Caja' })
    return Response.json({ idPresentacion: 2, descripcion: 'Caja', estado: true }, { status: 201 })
  })
  assert.equal((await createPresentation({ descripcion: ' Caja ' }, 'test-token')).idPresentacion, 2)
})
test('409 muestra duplicado; 401 y 403 conservan el estado para controlar sesión y permisos', async t => {
  for (const status of [409, 401, 403]) {
    t.mock.method(globalThis, 'fetch', async () => new Response('internal detail', { status }))
    await assert.rejects(createPresentation({ descripcion: 'Caja' }, 'test-token'), error =>
      error instanceof ApiError && error.status === status && !error.message.includes('internal') &&
      (status !== 409 || error.message.includes('Ya existe')))
    t.mock.restoreAll()
  }
})
test('No reintenta cuando se desconoce si el servidor guardó', async t => {
  const mocked = t.mock.method(globalThis, 'fetch', async () => { throw new TypeError('network') })
  await assert.rejects(createPresentation({ descripcion: 'Caja' }, 'test-token'), error =>
    error instanceof ApiError && error.message.includes('verifica si se guardó'))
  assert.equal(mocked.mock.callCount(), 1)
})
test('Listado usa GET autenticado y permite resultados vacíos', async t => {
  const rows = [{ idPresentacion: 1, descripcion: 'Unidad', estado: true }]
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Presentaciones')
    assert.equal(options.method, 'GET')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    return Response.json(rows)
  })
  assert.deepEqual(await listPresentations('token'), rows)
  rows.length = 0
  assert.deepEqual(await listPresentations('token'), [])
})
test('Listado rechaza estructuras inválidas e IDs repetidos', async t => {
  const row = { idPresentacion: 1, descripcion: 'Caja', estado: true }
  for (const data of [{}, [row, row], [{ ...row, estado: false }]]) {
    t.mock.method(globalThis, 'fetch', async () => Response.json(data))
    await assert.rejects(listPresentations('token'), error => error instanceof ApiError)
    t.mock.restoreAll()
  }
})
test('Actualizar usa PUT con ID, token y descripción, sin modificar estado', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Presentaciones/5')
    assert.equal(options.method, 'PUT')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    assert.deepEqual(JSON.parse(options.body as string), { descripcion: 'Media docena' })
    return Response.json({ idPresentacion: 5, descripcion: 'Media docena', estado: true })
  })
  assert.equal((await updatePresentation(5, { descripcion: ' Media docena ' }, 'token')).idPresentacion, 5)
})
test('Actualizar conserva errores de sesión, permisos, duplicados y registro inexistente', async t => {
  for (const status of [401, 403, 404, 409]) {
    t.mock.method(globalThis, 'fetch', async () => new Response('secret', { status }))
    await assert.rejects(updatePresentation(5, { descripcion: 'Caja' }, 'token'), error =>
      error instanceof ApiError && error.status === status && !error.message.includes('secret'))
    t.mock.restoreAll()
  }
})
test('Actualizar rechaza IDs inválidos antes de enviar y respuestas con otro ID', async t => {
  const mocked = t.mock.method(globalThis, 'fetch', async () => Response.json({ idPresentacion: 2, descripcion: 'Caja', estado: true }))
  await assert.rejects(updatePresentation(0, { descripcion: 'Caja' }, 'token'))
  assert.equal(mocked.mock.callCount(), 0)
  await assert.rejects(updatePresentation(5, { descripcion: 'Caja' }, 'token'))
})