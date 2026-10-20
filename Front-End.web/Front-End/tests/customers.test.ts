import test from 'node:test'
import assert from 'node:assert/strict'
import { loadSaleCustomer, loadSaleCustomers } from '../src/features/sales/api/customersApi.ts'
import { ApiError } from '../src/shared/api/ApiError.ts'

test('El índice de clientes conserva solo ID y nombre y consulta con autenticación', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Cliente/listar')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    assert.equal(options.cache, 'no-store')
    return Response.json([{ id_cliente: 1, nombre: 'Ana Pérez', nit: 'privado', telefono: 'privado', estado: false }])
  })
  assert.deepEqual(await loadSaleCustomers('token'), [{ id_cliente: 1, nombre: 'Ana Pérez' }])
})

test('El detalle se consulta por ID y no conserva campos ajenos al formulario', async t => {
  const detail = { id_cliente: 2, nombre: 'Luis', apellido: 'López', nit: '123', dpi: null, telefono: null, correo_electronico: null, direccion: 'Ciudad' }
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Cliente/2')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    return Response.json({ ...detail, limite_Credito: 999, estado: false })
  })
  assert.deepEqual(await loadSaleCustomer(2, 'token'), detail)
})

test('Respuestas inválidas o de otro cliente no llenan el formulario', async t => {
  let payload: unknown = [{ id_cliente: 1, nombre: 'Ana' }, { id_cliente: 1, nombre: 'Otra' }]
  t.mock.method(globalThis, 'fetch', async () => Response.json(payload))
  await assert.rejects(loadSaleCustomers('token'), ApiError)
  payload = { id_cliente: 3, nombre: 'Otro', apellido: null, nit: null, dpi: null, telefono: null, correo_electronico: null, direccion: null }
  await assert.rejects(loadSaleCustomer(2, 'token'), ApiError)
})

test('El cliente inexistente conserva 404 y los identificadores inválidos no consultan', async t => {
  let calls = 0
  t.mock.method(globalThis, 'fetch', async () => { calls++; return Response.json({}, { status: 404 }) })
  await assert.rejects(loadSaleCustomer(0, 'token'), (error: unknown) => error instanceof ApiError && error.status === 400)
  assert.equal(calls, 0)
  await assert.rejects(loadSaleCustomer(1, 'token'), (error: unknown) => error instanceof ApiError && error.status === 404)
  assert.equal(calls, 1)
})

test('Una consulta cancelada propaga la cancelación sin entregar datos', async t => {
  t.mock.method(globalThis, 'fetch', async (_: unknown, options: RequestInit) => {
    const signal = options.signal!
    return await new Promise<Response>((_resolve, reject) => {
      if (signal.aborted) reject(signal.reason)
      else signal.addEventListener('abort', () => reject(signal.reason), { once: true })
    })
  })
  const controller = new AbortController()
  const pending = loadSaleCustomer(1, 'token', controller.signal)
  controller.abort()
  await assert.rejects(pending)
})
