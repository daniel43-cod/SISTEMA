import test from 'node:test'
import assert from 'node:assert/strict'
import { lookupReceiver } from '../src/features/sales/api/receptorLookupApi.ts'

test('Consulta fiscal envía documento al backend sin credenciales de INFILE', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Fel/receptor/consultar')
    assert.equal(options.method, 'POST')
    assert.deepEqual(JSON.parse(options.body as string), { tipo: 'CUI', identificacion: '2993930620101' })
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer sesion')
    return Response.json({ resultado: true, tipo: 'CUI', identificacion: '2993930620101', nombre: ' NOMBRE REGISTRADO ' })
  })
  assert.equal(await lookupReceiver('CUI', '2993930620101', 'sesion'), 'NOMBRE REGISTRADO')
})
test('No usa nombre de otro documento ni inventa uno si no hay coincidencia', async t => {
  t.mock.method(globalThis, 'fetch', async () => Response.json({ resultado: true, tipo: 'NIT', identificacion: '999', nombre: 'OTRA PERSONA' }))
  await assert.rejects(lookupReceiver('NIT', '1170142K', 'sesion'), /no corresponde/)
})
test('NIT no encontrado permite mostrar un mensaje sin guardar nombre', async t => {
  t.mock.method(globalThis, 'fetch', async () => Response.json({ resultado: false, tipo: 'NIT', identificacion: '1170142K', nombre: null }))
  await assert.rejects(lookupReceiver('NIT', '1170142K', 'sesion'), /Revisa el NIT/)
})
