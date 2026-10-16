import assert from 'node:assert/strict'
import test from 'node:test'
import { parseCustomerSession, validateCustomerCredentials } from '../src/features/customer-auth/schemas/customerSession.ts'
const token = (payload: object) => 'header.' + Buffer.from(JSON.stringify(payload)).toString('base64url') + '.signature'
const response = (payload: object) => ({ idCuentaCliente: 5, correoElectronico: 'cliente@test.com', token: token(payload) })
const claims = { sub: '5', tipo_cuenta: 'cuenta_cliente', exp: Math.floor(Date.now() / 1000) + 3600 }
test('Parses customer response and reads expiration from JWT', () => {
  const result = parseCustomerSession(response(claims))
  assert.equal(result.idCuentaCliente, 5)
  assert.equal(result.expiresAt, claims.exp * 1000)
})
test('Rejects employee tokens, mismatched account and expired tokens', () => {
  for (const override of [{ tipo_cuenta: 'usuario' }, { sub: '9' }, { exp: 1 }])
    assert.throws(() => parseCustomerSession(response({ ...claims, ...override })))
  assert.throws(() => parseCustomerSession({}))
})
test('Validates email and password including bcrypt byte limit', () => {
  assert.equal(validateCustomerCredentials({ correoElectronico: ' user@test.com ', password: 'secret' }), null)
  assert.ok(validateCustomerCredentials({ correoElectronico: 'wrong', password: 'secret' }))
  assert.ok(validateCustomerCredentials({ correoElectronico: 'user@test.com', password: '' }))
  assert.ok(validateCustomerCredentials({ correoElectronico: 'user@test.com', password: 'ñ'.repeat(37) }))
})
