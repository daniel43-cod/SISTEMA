import { test } from 'node:test'
import assert from 'node:assert/strict'
import { cashAgeHours, cashDay, groupCashMovements } from '../src/features/cash/utils/cashMovementView.ts'
import type { CashMovement } from '../src/features/cash/api/cashApi.ts'
function movement(id: number, date: string, amount: number, nature = 'ENTRADA'): CashMovement {
  return { id_movimiento_caja: id, id_sesion_caja: 1, tipo_movimiento: 'Prueba', naturaleza: nature,
    usuario: 'Usuario', fecha_movimiento: date, monto: amount, descripcion: null }
}
test('El aviso comienza a las 24 horas e interpreta fechas sin zona como Guatemala', () => {
  const opening = '2026-11-01T10:00:00'
  assert.equal(cashAgeHours(opening, Date.parse('2026-11-02T15:59:00Z')), 23)
  assert.equal(cashAgeHours(opening, Date.parse('2026-11-02T16:00:00Z')), 24)
  assert.equal(cashDay('2026-11-02T02:00:00Z'), cashDay('2026-11-01T20:00:00'))
})
test('Agrupa por día local y calcula netos con entradas y salidas en centavos', () => {
  const rows = [movement(1, '2026-11-01T20:00:00', .1), movement(2, '2026-11-01T21:00:00', .2),
    movement(3, '2026-11-02T02:00:00', 5, 'SALIDA')]
  const groups = groupCashMovements(rows, true)
  assert.equal(groups.length, 2)
  assert.deepEqual(groups[0].items.map(m => m.id_movimiento_caja), [3])
  assert.equal(groups[0].net, -5)
  assert.equal(groups[1].net, .3)
  assert.deepEqual(rows.map(m => m.id_movimiento_caja), [1, 2, 3])
})
test('Sin agrupación muestra una lista continua y ordena fechas iguales por identificador', () => {
  const rows = [movement(1, '2026-11-01T20:00:00', 1), movement(2, '2026-11-01T20:00:00', 2),
    movement(3, '2026-11-02T02:00:00', 3)]
  const groups = groupCashMovements(rows, false)
  assert.equal(groups.length, 1)
  assert.equal(groups[0].key, 'all')
  assert.deepEqual(groups[0].items.map(m => m.id_movimiento_caja), [3, 2, 1])
})