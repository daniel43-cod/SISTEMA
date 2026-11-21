import test from 'node:test'
import assert from 'node:assert/strict'
import { certifySale, loadFel, loadFelPreparation, parseFelDocument, recoverFel } from '../src/features/sales/api/felApi.ts'

const invoice = { id_documento_fel: 7, id_venta: 1141, ambiente: 'PRUEBAS', estado: 'CERTIFICADA',
  uuid: '15B0C102-4DD4-4ED6-8C5C-85B9515720B7', serie: '**PRUEBAS**', numero: '1305759446',
  pdf: 'https://report.feel.com.gt/ingfacereport/ingfacereport_documento?uuid=15B0C102-4DD4-4ED6-8C5C-85B9515720B7', errores: [], alertas: [] }

test('FEL: certificación explícita envía una sola petición autenticada', async t => {
  let calls = 0
  const body = { direccion: 'Ciudad', codigo_postal: '01001', municipio: 'Guatemala', departamento: 'Guatemala', pais: 'GT', confirmar_bienes_gravados: true }
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    calls++
    assert.equal(url, '/api/Venta/1141/certificar')
    assert.equal(options.method, 'POST')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer test')
    assert.deepEqual(JSON.parse(options.body as string), body)
    return Response.json(invoice)
  })
  assert.equal((await certifySale(1141, body, 'test')).estado, 'CERTIFICADA')
  assert.equal(calls, 1)
})

test('FEL: CF se envía sin datos de dirección', async t => {
  t.mock.method(globalThis, 'fetch', async (_url: string, options: RequestInit) => {
    assert.deepEqual(JSON.parse(options.body as string), { confirmar_bienes_gravados: true })
    return Response.json(invoice)
  })
  await certifySale(1141, { confirmar_bienes_gravados: true }, 'test')
})

test('FEL: consulta no emite factura y conserva mensajes de rechazo', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Venta/1141/fel')
    assert.equal(options.method, 'GET')
    assert.equal(options.body, undefined)
    return Response.json({ documento: { ...invoice, estado: 'RECHAZADA', pdf: null, errores: [{ fuente: 'INFILE', mensaje: 'Revisar NIT' }] } })
  })
  assert.equal((await loadFel(1141, 'test'))?.errores[0].mensaje, 'Revisar NIT')
})

test('FEL: rechaza resultados de otra venta y enlaces PDF ajenos a INFILE', () => {
  assert.throws(() => parseFelDocument(invoice, 99))
  assert.throws(() => parseFelDocument({ ...invoice, pdf: 'https://example.com/factura' }, 1141))
})

test('Preparar certificación obtiene el total sin emitir una factura', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Venta/1141/fel/preparacion')
    assert.equal(options.method, 'GET')
    return Response.json({ total: 3000, tipo_identificacion_receptor: null })
  })
  assert.equal((await loadFelPreparation(1141, 'test')).total, 3000)
})

test('La selección fiscal se envía al certificar, sin nombre arbitrario', async t => {
  t.mock.method(globalThis, 'fetch', async (_url: string, options: RequestInit) => {
    assert.deepEqual(JSON.parse(options.body as string), {
      tipo_identificacion_receptor: 'CF', identificacion_receptor: 'CF', confirmar_bienes_gravados: true,
    })
    return Response.json(invoice)
  })
  await certifySale(1141, { tipo_identificacion_receptor: 'CF', identificacion_receptor: 'CF', confirmar_bienes_gravados: true }, 'test')
})
test('Recuperar certificación no permite cambiar XML, receptor ni identificador', async t => {
  let calls = 0
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    calls++
    assert.equal(url, '/api/Venta/1141/fel/recuperar')
    assert.equal(options.method, 'POST')
    assert.equal(options.body, undefined)
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer test')
    return Response.json({ ...invoice, puede_recuperar: false, recuperaciones: 1 })
  })
  assert.equal((await recoverFel(1141, 'test')).estado, 'CERTIFICADA')
  assert.equal(calls, 1)
})
