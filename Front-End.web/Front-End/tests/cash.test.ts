import { test } from 'node:test'
import assert from 'node:assert/strict'
import { loadCash, openCash, parseOpeningAmount, parseAvailableCash, parseCashSession } from '../src/features/cash/api/cashApi.ts'
import { ApiError } from '../src/shared/api/ApiError.ts'

test('Monto inicial acepta cero, coma decimal y limite SQL; rechaza formatos ambiguos', () => {
  assert.equal(parseOpeningAmount('0'), 0)
  assert.equal(parseOpeningAmount('125,50'), 125.5)
  assert.equal(parseOpeningAmount('99999999.99'), 99999999.99)
  for (const value of ['', '-1', '1.001', '100000000', '1e2', '1x2', 'NaN', '1,000.00'])
    assert.equal(parseOpeningAmount(value), null)
})

test('Contratos invalidos nunca habilitan apertura accidental', () => {
  assert.equal(parseCashSession(null), null)
  assert.deepEqual(parseAvailableCash([]), [])
  for (const value of [{}, { id_sesion_caja: 1, id_caja: 1 }, undefined]) assert.throws(() => parseCashSession(value), ApiError)
  assert.throws(() => parseAvailableCash([{ idCaja: 0, nombre: 'Caja' }]), ApiError)
  assert.throws(() => parseAvailableCash([{ idCaja: 1, nombre: 'Caja' }, { idCaja: 1, nombre: 'Caja' }]), ApiError)
})

test('Consulta caja actual y disponibles con token y acepta caja cerrada', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    if (url === '/api/Caja/actual') return new Response(null, { status: 204 })
    assert.equal(url, '/api/Caja/disponibles')
    return Response.json([{ idCaja: 1, nombre: 'Principal' }])
  })
  assert.deepEqual(await loadCash('token'), { current: null, available: [{ idCaja: 1, nombre: 'Principal' }] })
})

test('Apertura envia solo caja, monto y observacion, nunca el responsable', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Caja/abrir')
    assert.equal(options.method, 'POST')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    assert.deepEqual(JSON.parse(options.body as string), { id_caja: 1, monto_inicial: 100.5, observacion: 'Turno' })
    return Response.json({ id_sesion_caja: 3 })
  })
  await openCash('token', 1, 100.5, ' Turno ')
})

test('Apertura conserva rechazo por permisos y no expone errores internos', async t => {
  for (const status of [403, 409, 500]) {
    t.mock.method(globalThis, 'fetch', async () => new Response('SQL password=secret', { status }))
    await assert.rejects(openCash('token', 1, 0, ''), error => error instanceof ApiError && error.status === status && !error.message.includes('secret'))
  }
})

test('Cierre envia turno revisado y efectivo, no responsable ni monto esperado', async t => {
  const { closeCash } = await import('../src/features/cash/api/cashApi.ts')
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Caja/cerrar')
    assert.equal(options.method, 'POST')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    assert.deepEqual(JSON.parse(options.body as string), { id_sesion_caja: 7, monto_contado: 90, observacion_cierre: 'Conteo' })
    return Response.json({ id_sesion_caja: 7, id_usuario_apertura: 1, id_usuario_cierre: 2,
      fecha_cierre: '2026-10-07T18:00:00', monto_inicial: 100, monto_esperado: 100, monto_contado: 90, diferencia: -10 })
  })
  const result = await closeCash('token', 7, 90, ' Conteo ')
  assert.equal(result.diferencia, -10)
  assert.equal(result.id_usuario_cierre, 2)
})

test('Cierre rechaza datos invalidos sin consultar el servidor', async t => {
  const { closeCash } = await import('../src/features/cash/api/cashApi.ts')
  const fetch = t.mock.method(globalThis, 'fetch', async () => { throw new Error('No debe consultarse') })
  for (const [id, amount] of [[0, 1], [1, -1], [1, 1.001], [1, NaN], [1, 100000000]])
    await assert.rejects(closeCash('token', id, amount, ''), ApiError)
  await assert.rejects(closeCash('token', 1, 0, 'a'.repeat(101)), ApiError)
  assert.equal(fetch.mock.callCount(), 0)
})

test('Cierre rechaza respuesta invalida y resultado de otro turno', async t => {
  const { closeCash, parseCashClosing } = await import('../src/features/cash/api/cashApi.ts')
  assert.throws(() => parseCashClosing({}), ApiError)
  t.mock.method(globalThis, 'fetch', async () => Response.json({ id_sesion_caja: 8, id_usuario_apertura: 1,
    id_usuario_cierre: 2, fecha_cierre: '2026-10-07T18:00:00', monto_inicial: 0, monto_esperado: 0,
    monto_contado: 0, diferencia: 0 }))
  await assert.rejects(closeCash('token', 7, 0, ''), ApiError)
})

test('Cierre conserva conflicto o falta de permisos y nunca reintenta automaticamente', async t => {
  const { closeCash } = await import('../src/features/cash/api/cashApi.ts')
  for (const status of [400, 403, 409, 429, 500]) {
    const fetch = t.mock.method(globalThis, 'fetch', async () => new Response('SQL secret', { status }))
    await assert.rejects(closeCash('token', 7, 0, ''), error => error instanceof ApiError && error.status === status && !error.message.includes('secret'))
    assert.equal(fetch.mock.callCount(), 1)
    fetch.mock.restore()
  }
})
