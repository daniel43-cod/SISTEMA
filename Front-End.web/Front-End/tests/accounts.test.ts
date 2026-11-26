import test from 'node:test'
import assert from 'node:assert/strict'
import { loadAccounts, parseAccounts } from '../src/features/accounts/api/accountsApi.ts'
import { getStaffNavigation } from '../src/app/navigation/navigationItems.ts'

const row = { id: 1, nombre: 'Cliente', usuario: 'Admin', fecha: '2026-11-01T10:00:00', total: 100, saldo: 60, subtotal: 100, descuento: null, ganancia: 10 }
const page = { totalPorCobrar: 160, totalPorPagar: 200, totalRegistros: 4, saldoFiltrado: 160, hayMas: true, items: [row] }
test('Cuentas: acceso al menú solo para administradores', () => {
  assert.ok(getStaffNavigation('ADMINISTRADOR').some(i => i.id === 'cuentas'))
  assert.ok(!getStaffNavigation('VENDEDOR').some(i => i.id === 'cuentas'))
})
test('Cuentas: contrato de saldo parcial y página vacía', () => {
  assert.equal(parseAccounts(page).items[0].saldo, 60)
  assert.equal(parseAccounts({ ...page, items: [], totalRegistros: 0, saldoFiltrado: 0, hayMas: false }).items.length, 0)
})
for (const change of [{ saldo: 0 }, { saldo: 101 }, { saldo: -1 }, { saldo: NaN }, { fecha: 'inválida' }, { id: 0 }])
  test('Cuentas: rechaza cuenta inválida ' + JSON.stringify(change), () => assert.throws(() => parseAccounts({ ...page, items: [{ ...row, ...change }] })))
test('Cuentas: rechaza duplicados y totales inválidos', () => {
  assert.throws(() => parseAccounts({ ...page, items: [row, row] }))
  assert.throws(() => parseAccounts({ ...page, totalPorCobrar: -1 }))
})
for (const tipo of ['cobrar', 'pagar'] as const) test('Cuentas: consulta paginada ' + tipo + ' sin filtro por caja o fecha', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    const parsed = new URL(url, 'https://local.test')
    assert.equal(parsed.pathname, '/api/Cuentas')
    assert.equal(parsed.searchParams.get('tipo'), tipo)
    assert.equal(parsed.searchParams.get('pagina'), '2')
    assert.equal(parsed.searchParams.get('buscar'), 'Cliente')
    assert.equal((options.headers as Record<string,string>).Authorization, 'Bearer prueba')
    assert.equal(parsed.searchParams.has('idSesionCaja'), false)
    return new Response(JSON.stringify(page), { status: 200, headers: { 'Content-Type': 'application/json' } })
  })
  assert.equal((await loadAccounts('prueba', tipo, 2, 'Cliente')).totalRegistros, 4)
})
