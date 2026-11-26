import test from 'node:test'
import assert from 'node:assert/strict'
import { parseClients, parseClientSummary, parseClientSales, loadClients, loadClientSummary, loadClientSales } from '../src/features/clients/api/clientsApi.ts'
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
