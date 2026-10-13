const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname, '../../Front-End.web/Front-End');
const api = path.join(root, 'src/features/cash/api/cashApi.ts');
let content = fs.readFileSync(api, 'utf8').replace("api/httpClient'", "api/httpClient.ts'").replace("api/ApiError'", "api/ApiError.ts'");
content = content.replace('return value as AvailableCash[]', "if (new Set(value.map(item => item.idCaja)).size !== value.length) throw new ApiError('Las cajas disponibles contienen duplicados.')\n  return value as AvailableCash[]");
fs.writeFileSync(api, content);
fs.writeFileSync(path.join(root, 'tests/cash.test.ts'), `import { test } from 'node:test'
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
`);
const pkg = path.join(root, 'package.json');
const data = JSON.parse(fs.readFileSync(pkg, 'utf8'));
if (!data.scripts.test.includes('tests/cash.test.ts')) data.scripts.test += ' tests/cash.test.ts';
fs.writeFileSync(pkg, JSON.stringify(data, null, 2) + '\n');
