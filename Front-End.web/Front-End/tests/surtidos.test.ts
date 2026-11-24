import { test } from 'node:test'
import assert from 'node:assert/strict'
import { createSurtido } from '../src/features/products/api/surtidosApi.ts'
import { ApiError } from '../src/shared/api/ApiError.ts'

const data = { nombre: 'Cola surtida', idMarca: 1, idsProductos: [2, 3],
  presentaciones: [{ idPresentacion: 4, unidadesEquivalentes: 6, precio: 16.2 }] }

test('Surtido envía selección y presentaciones autenticadas sin calcular costos en el cliente', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/surtidos')
    assert.equal(options.method, 'POST')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    assert.deepEqual(JSON.parse(options.body as string), data)
    return Response.json({ idSurtido: 8 }, { status: 201 })
  })
  assert.equal(await createSurtido(data, 'token'), 8)
})

test('Surtido muestra validación de compatibilidad del servidor sin reintentar el guardado', async t => {
  const mock = t.mock.method(globalThis, 'fetch', async () => Response.json({ mensaje: 'El precio mínimo es Q16.20.' }, { status: 400 }))
  await assert.rejects(createSurtido(data, 'token'), e => e instanceof ApiError && e.status === 400 && e.message === 'El precio mínimo es Q16.20.')
  assert.equal(mock.mock.callCount(), 1)
})

test('Surtido no confirma éxito ante una respuesta sin identificador válido', async t => {
  t.mock.method(globalThis, 'fetch', async () => Response.json({ idSurtido: 0 }))
  await assert.rejects(createSurtido(data, 'token'), /No se pudo confirmar el registro/)
})
