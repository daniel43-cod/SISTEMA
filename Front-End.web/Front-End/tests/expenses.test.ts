import assert from 'node:assert/strict'
import test from 'node:test'
import { buildExpense } from '../src/features/expenses/utils/expenseValidation.ts'
test('Normalizes description, optional note and decimal separator', () => {
  assert.deepEqual(buildExpense(' Transporte ', '12,50', ' Entrega '), { descripcion: 'Transporte', monto: 12.5, observacion: 'Entrega' })
  assert.equal(buildExpense('Transporte', '1', '').observacion, null)
})
test('Rejects empty or oversized descriptions and notes', () => {
  assert.throws(() => buildExpense('   ', '1', ''))
  assert.throws(() => buildExpense('A'.repeat(301), '1', ''))
  assert.throws(() => buildExpense('Transporte', '1', 'A'.repeat(501)))
})
test('Rejects invalid money and accepts boundary amounts', () => {
  for (const value of ['0', '-1', '1.001', '100000000', 'NaN', '']) assert.throws(() => buildExpense('Transporte', value, ''))
  assert.equal(buildExpense('Transporte', '0.01', '').monto, 0.01)
  assert.equal(buildExpense('Transporte', '99999999.99', '').monto, 99999999.99)
})
