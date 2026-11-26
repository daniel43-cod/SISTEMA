import test from 'node:test'
import assert from 'node:assert/strict'
import { parseClients, parseClientSummary, parseClientSales, updateClient, clientPhoneInput, validClientPhone, clientDocumentInput, validClientDocument, loadClients, loadClientSummary, loadClientSales } from '../src/features/clients/api/clientsApi.ts'
import { getStaffNavigation } from '../src/app/navigation/navigationItems.ts'

const client = { idCliente: 1, nombre: 'María López', nit: null, telefono: '55555555', direccion: 'Zona 1', estado: true }
const list = { totalRegistros: 1, pagina: 1, tamanoPagina: 30, hayMas: false, items: [client] }
const summary = { cliente: { id_cliente: 1, nombre: 'María', apellido: 'López', nit: null, dpi: null, telefono: null, correo_electronico: null, direccion: null }, estado: true, cantidadVentas: 1, totalComprado: 100, totalPagado: 40, saldoPendiente: 60 }
const sale = { idVenta: 7, fecha: '2026-11-01T10:00:00', idUsuario: 2, responsable: 'Admin', total: 100, pagado: 40, saldo: 60, idEstadoVenta: 2, estado: 'Pendiente' }
const sales = { ...list, idCliente: 1, items: [sale] }
test('Clientes: menú exclusivo de administradores', () => {
  assert.ok(getStaffNavigation('ADMINISTRADOR').some(i => i.id === 'clientes'))
  assert.ok(!getStaffNavigation('VENDEDOR').some(i => i.id === 'clientes'))
})
test('Clientes: lista, inactivos y página vacía', () => {
  assert.equal(parseClients(list).items[0].nombre, 'María López')
  assert.equal(parseClients({ ...list, items: [{ ...client, estado: false }] }).items[0].estado, false)
  assert.equal(parseClients({ ...list, totalRegistros: 0, items: [] }).items.length, 0)
})
test('Clientes: paginación de 30 y última página', () => {
  const items = Array.from({ length: 30 }, (_, i) => ({ ...client, idCliente: i + 1 }))
  assert.equal(parseClients({ ...list, totalRegistros: 31, hayMas: true, items }).items.length, 30)
  assert.equal(parseClients({ ...list, pagina: 2, totalRegistros: 31 }).items.length, 1)
})
for (const invalid of [null, { ...list, pagina: 0 }, { ...list, hayMas: true }, { ...list, totalRegistros: -1 }, { ...list, items: [{ ...client, idCliente: 0 }] }, { ...list, totalRegistros: 2, items: [client, client] }])
  test('Clientes: rechaza listado inválido ' + JSON.stringify(invalid), () => assert.throws(() => parseClients(invalid)))
test('Clientes: resumen con datos opcionales vacíos y totales cero', () => {
  assert.equal(parseClientSummary(summary, 1).saldoPendiente, 60)
  assert.equal(parseClientSummary({ ...summary, cantidadVentas: 0, totalComprado: 0, totalPagado: 0, saldoPendiente: 0 }, 1).cantidadVentas, 0)
  assert.throws(() => parseClientSummary(summary, 2))
  assert.throws(() => parseClientSummary({ ...summary, saldoPendiente: NaN }, 1))
})
test('Clientes: historial identifica cliente y valida ventas', () => {
  assert.equal(parseClientSales(sales, 1).items[0].idVenta, 7)
  assert.throws(() => parseClientSales(sales, 2))
  for (const change of [{ fecha: 'inválida' }, { saldo: 101 }, { pagado: -1 }, { idVenta: 0 }])
    assert.throws(() => parseClientSales({ ...sales, items: [{ ...sale, ...change }] }, 1))
})
test('Clientes: usa rutas administrativas y transmite cancelación y búsqueda', async () => {
  const original = globalThis.fetch
  const calls: string[] = []
  const signals: AbortSignal[] = []
  const abort = new AbortController()
  globalThis.fetch = (async (input, init) => {
    const url = String(input); calls.push(url)
    assert.ok(init?.signal instanceof AbortSignal)
    signals.push(init.signal)
    assert.equal((init.headers as Record<string, string>).Authorization, 'Bearer token')
    return new Response(JSON.stringify(url.includes('/resumen') ? summary : url.includes('/ventas') ? sales : list), { status: 200, headers: { 'Content-Type': 'application/json' } })
  }) as typeof fetch
  try {
    await loadClients('token', 1, 'María & López', abort.signal)
    await loadClientSummary('token', 1, abort.signal)
    await loadClientSales('token', 1, 1, abort.signal)
    assert.ok(calls[0].includes('tamanoPagina=30') && calls[0].includes('Mar%C3%ADa+%26+L%C3%B3pez'))
    assert.ok(calls[1].includes('/clientes/1/resumen'))
    assert.ok(calls[2].includes('/clientes/1/ventas?pagina=1'))
    assert.ok(!calls[2].includes('fecha='))
    await loadClientSales('token', 1, 1, abort.signal, '2026-11-01')
    assert.ok(calls[3].includes('fecha=2026-11-01'))
    abort.abort()
    assert.ok(signals.every(s => s.aborted))
  } finally { globalThis.fetch = original }
})

test('Clientes: acepta API anterior sin dirección y normaliza a null', () => {
  const { direccion, ...previous } = client
  assert.equal(parseClients({ ...list, items: [previous] }).items[0].direccion, null)
  assert.equal(parseClients(list).items[0].direccion, direccion)
  assert.throws(() => parseClients({ ...list, items: [{ ...client, direccion: 123 }] }))
})

test('Clientes: edición usa PUT autenticado y devuelve resumen del mismo cliente', async () => {
  const original = globalThis.fetch
  const datos = { nombre: 'María', apellido: 'López', nit: '', dpi: '', telefono: '55555555', correoElectronico: '', direccion: 'Zona 1' }
  globalThis.fetch = (async (input, init) => {
    assert.ok(String(input).endsWith('/clientes/1'))
    assert.equal(init?.method, 'PUT')
    assert.equal((init?.headers as Record<string, string>).Authorization, 'Bearer token')
    assert.deepEqual(JSON.parse(init?.body as string), datos)
    return new Response(JSON.stringify(summary), { status: 200 })
  }) as typeof fetch
  try { assert.equal((await updateClient('token', 1, datos)).cliente.id_cliente, 1) }
  finally { globalThis.fetch = original }
})
test('Clientes: teléfono admite solo ocho dígitos o vacío', () => {
  for (const value of ['', '12345678', '00123456']) assert.ok(validClientPhone(value))
  for (const value of ['1', '1234567', '123456789', '1234567a', 'abcdefgh', '+50212345678', '1234 5678', '１２３４５６７８', ' ']) assert.ok(!validClientPhone(value))
  assert.equal(clientPhoneInput('12abc34'), '1234')
  assert.equal(clientPhoneInput('1234567890'), '12345678')
  assert.equal(clientPhoneInput(''), '')
})
test('Clientes: teléfono inválido impide enviar la edición', async () => {
  const original = globalThis.fetch
  let calls = 0
  globalThis.fetch = (async () => { calls++; throw new Error('No debe enviarse') }) as typeof fetch
  try {
    await assert.rejects(updateClient('token', 1, { nombre: 'Cliente', apellido: '', nit: '', dpi: '', telefono: '123', correoElectronico: '', direccion: '' }), /8 dígitos/)
    assert.equal(calls, 0)
  } finally { globalThis.fetch = original }
})
test('Clientes: DPI y NIT solo numéricos u opcionales conservan ceros iniciales', () => {
  for (const value of ['', '0012345', '1234567890123']) assert.ok(validClientDocument(value))
  for (const value of ['abc', '123A', '123K', '123-4', '1.2', '1e3', '１２３', ' ', '1'.repeat(21)]) assert.ok(!validClientDocument(value))
  assert.equal(clientDocumentInput('0012abc34'), '001234')
})
test('Clientes: documentos inválidos impiden enviar edición', async () => {
  const original = globalThis.fetch
  let calls = 0
  globalThis.fetch = (async () => { calls++; throw new Error('No enviar') }) as typeof fetch
  const datos = { nombre: 'Cliente', apellido: '', nit: '', dpi: '', telefono: '', correoElectronico: '', direccion: '' }
  try {
    await assert.rejects(updateClient('token', 1, { ...datos, dpi: '123A' }), /DPI/)
    await assert.rejects(updateClient('token', 1, { ...datos, nit: '123K' }), /NIT/)
    assert.equal(calls, 0)
  } finally { globalThis.fetch = original }
})