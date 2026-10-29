const fs = require('node:fs'), path = require('node:path');
const root = path.resolve(__dirname, '../../Front-End.web/Front-End');
function write(file, s) { fs.writeFileSync(path.join(root, file), s); }
write('src/features/customer-auth/schemas/customerRegistration.ts', `export function validateCustomerRegistration(email: string, password: string, confirmation: string): string | null {
  if (!/^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$/.test(email.trim()) || email.trim().length > 254) return 'Ingresa un correo electrónico válido.'
  if (password.length < 8 || !/[a-z]/.test(password) || !/[A-Z]/.test(password) || !/[0-9]/.test(password))
    return 'La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula y un número.'
  if (new TextEncoder().encode(password).length > 72) return 'La contraseña no puede superar 72 bytes en UTF-8.'
  if (password !== confirmation) return 'Las contraseñas no coinciden.'
  return null
}
`);
write('src/features/customer-auth/api/customerRegistrationApi.ts', `import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'
import { validateCustomerRegistration } from '../schemas/customerRegistration.ts'

export async function registerCustomer(email: string, password: string, confirmation: string, signal?: AbortSignal): Promise<void> {
  const validation = validateCustomerRegistration(email, password, confirmation)
  if (validation) throw new ApiError(validation, 400)
  try {
    const result = await requestJson('/CuentaCliente/registro', { method: 'POST', signal,
      body: { correoElectronico: email.trim().toLowerCase(), password } })
    if (!result || typeof result !== 'object' || !('idCuentaCliente' in result) ||
      !Number.isSafeInteger(result.idCuentaCliente) || Number(result.idCuentaCliente) <= 0)
      throw new ApiError('No se pudo confirmar el registro. Intenta iniciar sesión o verifica antes de volver a crear la cuenta.', 200)
  } catch (failure) {
    if (failure instanceof ApiError && failure.status === 409)
      throw new ApiError('Ese correo ya tiene una cuenta. Inicia sesión para continuar.', 409)
    throw failure
  }
}
`);
write('src/features/customer-auth/components/CustomerLoginForm.tsx', `import { useEffect, useId, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { loginCustomer } from '../api/customerAuthApi'
import { registerCustomer } from '../api/customerRegistrationApi'
import { validateCustomerCredentials } from '../schemas/customerSession'
import { validateCustomerRegistration } from '../schemas/customerRegistration'
import type { CustomerSession } from '../types/customerAuth'

export function CustomerLoginForm({ onLogin }: { onLogin: (session: CustomerSession) => void }) {
  const [registering, setRegistering] = useState(false)
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmation, setConfirmation] = useState('')
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [loading, setLoading] = useState(false)
  const busy = useRef(false)
  const controllerRef = useRef<AbortController | null>(null)
  const emailRef = useRef<HTMLInputElement>(null)
  const id = useId()
  useEffect(() => () => controllerRef.current?.abort(), [])
  function changeMode(value: boolean) {
    setRegistering(value); setPassword(''); setConfirmation(''); setError(''); setNotice('')
    emailRef.current?.focus()
  }
  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (busy.current) return
    const credentials = { correoElectronico: email, password }
    const validation = registering ? validateCustomerRegistration(email, password, confirmation) : validateCustomerCredentials(credentials)
    if (validation) { setError(validation); return }
    const controller = new AbortController()
    controllerRef.current = controller
    busy.current = true; setLoading(true); setError(''); setNotice('')
    try {
      if (registering) {
        await registerCustomer(email, password, confirmation, controller.signal)
        if (!controller.signal.aborted) {
          setPassword(''); setConfirmation(''); setRegistering(false)
          setNotice('Cuenta creada. Inicia sesión para continuar.'); emailRef.current?.focus()
        }
      } else {
        const session = await loginCustomer(credentials, controller.signal)
        if (!controller.signal.aborted) { setPassword(''); onLogin(session) }
      }
    } catch (failure) {
      if (!controller.signal.aborted) setError(failure instanceof Error ? failure.message : 'No se pudo completar la solicitud.')
    } finally {
      busy.current = false
      if (!controller.signal.aborted) setLoading(false)
    }
  }
  return <form className="customer-login-form" onSubmit={submit} aria-busy={loading}>
    <p>{registering ? 'Crea tu cuenta para usar el asistente.' : 'Iniciá sesión con tu cuenta de cliente.'}</p>
    {notice && <p className="customer-register-success" role="status">{notice}</p>}
    <fieldset disabled={loading}>
      <legend className="public-cart-announcement">{registering ? 'Crear cuenta de cliente' : 'Inicio de sesión de cliente'}</legend>
      <label htmlFor={id + '-email'}>Correo electrónico</label>
      <input ref={emailRef} id={id + '-email'} type="email" autoComplete="username" value={email}
        onChange={event => setEmail(event.target.value)} maxLength={254} required />
      <label htmlFor={id + '-password'}>Contraseña</label>
      <input id={id + '-password'} type="password" autoComplete={registering ? 'new-password' : 'current-password'} value={password}
        onChange={event => setPassword(event.target.value)} required aria-describedby={registering ? id + '-rules' : undefined} />
      {registering && <>
        <small id={id + '-rules'}>Al menos 8 caracteres, una mayúscula, una minúscula y un número.</small>
        <label htmlFor={id + '-confirm'}>Confirmar contraseña</label>
        <input id={id + '-confirm'} type="password" autoComplete="new-password" value={confirmation}
          onChange={event => setConfirmation(event.target.value)} required />
      </>}
      <button type="submit">{loading ? registering ? 'Creando cuenta…' : 'Iniciando sesión…' : registering ? 'Crear cuenta' : 'Iniciar sesión'}</button>
    </fieldset>
    {error && <p className="customer-login-error" role="alert">{error}</p>}
    <p className="customer-auth-switch">{registering ? '¿Ya tienes una cuenta?' : '¿No tienes una cuenta?'}{' '}
      <button type="button" disabled={loading} onClick={() => changeMode(!registering)}>{registering ? 'Inicia sesión' : 'Crear cuenta'}</button>
    </p>
  </form>
}
`);
fs.appendFileSync(path.join(root, 'src/features/public-catalog/pages/PublicCategoriesPage.css'), `
.customer-login-form .customer-auth-switch button { background: transparent; color: #1d4ed8; text-decoration: underline; margin: 0; padding: 4px 8px; cursor: pointer; }
.public-assistant-intro p.customer-register-success { padding: 10px; border-radius: 8px; background: #dcfce7; color: #166534; }
.customer-login-form small { color: #475569; line-height: 1.5; }
`);
fs.appendFileSync(path.join(root, 'tests/customers.test.ts'), `

import { registerCustomer } from '../src/features/customer-auth/api/customerRegistrationApi.ts'
import { validateCustomerRegistration } from '../src/features/customer-auth/schemas/customerRegistration.ts'

test('Registro de cliente valida correo, complejidad, confirmación y límite UTF-8', () => {
  assert.equal(validateCustomerRegistration('ana@example.com', 'Password123', 'Password123'), null)
  assert.ok(validateCustomerRegistration('invalido', 'Password123', 'Password123'))
  for (const password of ['123', 'password123', 'PASSWORD123', 'Password', 'Aa1' + 'á'.repeat(35)])
    assert.ok(validateCustomerRegistration('ana@example.com', password, password))
  assert.ok(validateCustomerRegistration('ana@example.com', 'Password123', 'Otra1234'))
})

test('Registro envía únicamente correo y contraseña sin iniciar sesión ni enviar token', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/CuentaCliente/registro')
    assert.equal(options.method, 'POST')
    assert.deepEqual(JSON.parse(options.body as string), { correoElectronico: 'ana@example.com', password: 'Password123' })
    assert.equal((options.headers as Record<string, string>).Authorization, undefined)
    return Response.json({ idCuentaCliente: 1, correoElectronico: 'ana@example.com' }, { status: 201 })
  })
  await registerCustomer(' Ana@example.com ', 'Password123', 'Password123')
})

test('Registro con correo existente indica iniciar sesión sin exponer detalles internos', async t => {
  t.mock.method(globalThis, 'fetch', async () => Response.json({ detail: 'SQL secreto' }, { status: 409 }))
  await assert.rejects(registerCustomer('ana@example.com', 'Password123', 'Password123'),
    (error: unknown) => error instanceof ApiError && error.status === 409 && error.message.includes('Inicia sesión') && !error.message.includes('SQL'))
})
`);
