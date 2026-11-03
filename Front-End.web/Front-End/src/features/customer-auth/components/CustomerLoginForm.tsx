import { useEffect, useId, useRef, useState } from 'react'
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
  const [showPassword, setShowPassword] = useState(false)
  const [showConfirmation, setShowConfirmation] = useState(false)
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
    setShowPassword(false); setShowConfirmation(false)
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
          setShowPassword(false); setShowConfirmation(false)
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
      <div className="customer-password-field">
      <input id={id + '-password'} type={showPassword ? 'text' : 'password'} autoComplete={registering ? 'new-password' : 'current-password'} value={password}
        onChange={event => setPassword(event.target.value)} required aria-describedby={registering ? id + '-rules' : undefined} />
      <PasswordToggle visible={showPassword} onToggle={() => setShowPassword(value => !value)} controls={id + '-password'} />
      </div>
      {registering && <>
        <small id={id + '-rules'}>Al menos 8 caracteres, una mayúscula, una minúscula y un número.</small>
        <label htmlFor={id + '-confirm'}>Confirmar contraseña</label>
        <div className="customer-password-field">
        <input id={id + '-confirm'} type={showConfirmation ? 'text' : 'password'} autoComplete="new-password" value={confirmation}
          onChange={event => setConfirmation(event.target.value)} required />
        <PasswordToggle visible={showConfirmation} onToggle={() => setShowConfirmation(value => !value)} controls={id + '-confirm'} confirmation />
        </div>
      </>}
      <button type="submit">{loading ? registering ? 'Creando cuenta…' : 'Iniciando sesión…' : registering ? 'Crear cuenta' : 'Iniciar sesión'}</button>
    </fieldset>
    {error && <p className="customer-login-error" role="alert">{error}</p>}
    <p className="customer-auth-switch">{registering ? '¿Ya tienes una cuenta?' : '¿No tienes una cuenta?'}{' '}
      <button type="button" disabled={loading} onClick={() => changeMode(!registering)}>{registering ? 'Inicia sesión' : 'Crear cuenta'}</button>
    </p>
  </form>
}

function PasswordToggle({ visible, onToggle, controls, confirmation = false }: {
  visible: boolean
  onToggle: () => void
  controls: string
  confirmation?: boolean
}) {
  const label = (visible ? 'Ocultar' : 'Mostrar') + ' ' + (confirmation ? 'confirmaci?n de contrase?a' : 'contrase?a')
  return <button className="customer-password-toggle" type="button" aria-label={label}
    title={label} aria-pressed={visible} aria-controls={controls} onClick={onToggle}>
    <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor"
      strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
      <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12Z" />
      <circle cx="12" cy="12" r="3" />
      {visible && <path d="m3 3 18 18" />}
    </svg>
  </button>
}
