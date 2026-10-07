import { test } from 'node:test'
import assert from 'node:assert/strict'
import { loginStaff } from '../src/features/auth/api/authApi.ts'
import { validateLogin, parseStaffSession } from '../src/features/auth/schemas/loginSchema.ts'
import { requestJson, parseRetryAfter } from '../src/shared/api/httpClient.ts'
import { ApiError } from '../src/shared/api/ApiError.ts'

const sessionResponse = (overrides: Record<string, unknown> = {}) => ({
  id_usuario: 1, nombre: 'José', rol: 'ADMINISTRADOR',
  token: 'header.' + Buffer.from(JSON.stringify({
    sub: '1', tipo_cuenta: 'usuario', exp: Math.floor(Date.now() / 1000) + 1800,
  })).toString('base64url') + '.signature',
  ...overrides,
})
const credentials = { usuario: ' admin ', password: ' Password123 ' }

test('Valida vacíos, longitud y bytes UTF-8 sin truncar contraseña', () => {
  assert.deepEqual(validateLogin({ usuario: '', password: '' }), {
    usuario: 'Ingresa tu usuario.', password: 'Ingresa tu contraseña.',
  })
  assert.ok(validateLogin({ usuario: 'x'.repeat(51), password: 'a' }).usuario)
  assert.ok(validateLogin({ usuario: 'admin', password: 'á'.repeat(37) }).password)
  assert.deepEqual(validateLogin({ usuario: 'admin', password: 'á'.repeat(36) }), {})
})

test('Acepta roles internos y rechaza respuestas, tipos y JWT inválidos', () => {
  assert.equal(parseStaffSession(sessionResponse()).user.name, 'José')
  assert.equal(parseStaffSession(sessionResponse({ rol: 'VENDEDOR' })).user.role, 'VENDEDOR')
  for (const data of [null, {}, sessionResponse({ rol: 'CLIENTE' }), sessionResponse({ id_usuario: '1' }),
    sessionResponse({ token: 'invalid' }), sessionResponse({ id_usuario: 2 })])
    assert.throws(() => parseStaffSession(data))
  const expired = 'h.' + Buffer.from(JSON.stringify({ sub: '1', tipo_cuenta: 'usuario', exp: 1 })).toString('base64url') + '.s'
  assert.throws(() => parseStaffSession(sessionResponse({ token: expired })))
})

test('Envía el contrato correcto sin cookies, caché o modificación de contraseña', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, init: RequestInit) => {
    assert.equal(url, '/api/Login/Login')
    assert.equal(init.method, 'POST')
    assert.equal(init.cache, 'no-store')
    assert.equal(init.credentials, 'omit')
    assert.equal(init.redirect, 'error')
    assert.deepEqual(JSON.parse(init.body as string), { usuario: 'admin', password: ' Password123 ' })
    assert.equal((init.headers as Record<string, string>).Authorization, undefined)
    return Response.json(sessionResponse())
  })
  assert.equal((await loginStaff(credentials)).user.id, 1)
})

test('401 muestra mensaje de credenciales, sin detalles internos', async t => {
  t.mock.method(globalThis, 'fetch', async () => new Response('SQL secret', { status: 401 }))
  await assert.rejects(loginStaff(credentials), e => e instanceof ApiError &&
    e.message === 'Usuario o contraseña incorrectos.' && e.status === 401)
})

test('429 respeta Retry-After y no reintenta automáticamente', async t => {
  const mock = t.mock.method(globalThis, 'fetch', async () => new Response('', { status: 429, headers: { 'Retry-After': '75' } }))
  await assert.rejects(loginStaff(credentials), e => e instanceof ApiError && e.retryAfterSeconds === 75)
  assert.equal(mock.mock.callCount(), 1)
  assert.equal(parseRetryAfter(null), 60)
  assert.equal(parseRetryAfter('bad'), 60)
  assert.ok(parseRetryAfter(new Date(Date.now() + 45000).toUTCString()) <= 45)
})

test('400, 403 y 500 producen errores seguros', async t => {
  for (const status of [400, 403, 500]) {
    t.mock.method(globalThis, 'fetch', async () => new Response('SQL password=secret', { status }))
    await assert.rejects(loginStaff(credentials), e => e instanceof ApiError &&
      e.status === status && !e.message.includes('secret'))
    t.mock.restoreAll()
  }
})

test('Rechaza JSON inválido y respuestas HTML de proxies', async t => {
  t.mock.method(globalThis, 'fetch', async () => new Response('<html>error</html>'))
  await assert.rejects(loginStaff(credentials), e => e instanceof ApiError)
})

test('Informa fallo de conexión sin exponer detalles', async t => {
  t.mock.method(globalThis, 'fetch', async () => { throw new TypeError('internal network detail') })
  await assert.rejects(loginStaff(credentials), e => e instanceof ApiError && e.message.includes('conectar'))
})

test('Propaga cancelación del formulario', async t => {
  const controller = new AbortController()
  controller.abort()
  t.mock.method(globalThis, 'fetch', async () => { throw controller.signal.reason })
  await assert.rejects(loginStaff(credentials, controller.signal), { name: 'AbortError' })
})

test('No envía tokens a URLs arbitrarias', async t => {
  const mock = t.mock.method(globalThis, 'fetch', async () => Response.json({}))
  await assert.rejects(requestJson('https://other.example/', { token: 'secret' }))
  await assert.rejects(requestJson('//other.example/', { token: 'secret' }))
  assert.equal(mock.mock.callCount(), 0)
})
test('La duración la define el backend: admite sesiones superiores a 60 minutos', () => {
  const tokenWithMinutes = (minutes: number) => 'h.' + Buffer.from(JSON.stringify({
    sub: '1', tipo_cuenta: 'usuario', id_sesion: 'b765f4e4-e04b-405b-b53e-68a7b3c86dd8',
    exp: Math.floor(Date.now() / 1000) + minutes * 60,
  })).toString('base64url') + '.s'
  assert.ok(parseStaffSession(sessionResponse({ token: tokenWithMinutes(60) })).expiresAt > Date.now())
  assert.ok(parseStaffSession(sessionResponse({ token: tokenWithMinutes(120) })).expiresAt > Date.now())
})

