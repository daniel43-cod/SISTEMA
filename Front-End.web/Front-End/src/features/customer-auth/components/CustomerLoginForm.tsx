import { useEffect, useId, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { loginCustomer } from '../api/customerAuthApi'
import { validateCustomerCredentials } from '../schemas/customerSession'
import type { CustomerSession } from '../types/customerAuth'

export function CustomerLoginForm({ onLogin }: { onLogin: (session: CustomerSession) => void }) {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const busy = useRef(false)
  const controllerRef = useRef<AbortController | null>(null)
  const id = useId()
  useEffect(() => () => controllerRef.current?.abort(), [])
  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (busy.current) return
    const credentials = { correoElectronico: email, password }
    const validation = validateCustomerCredentials(credentials)
    if (validation) { setError(validation); return }
    const controller = new AbortController()
    controllerRef.current = controller
    busy.current = true; setLoading(true); setError('')
    try {
      const session = await loginCustomer(credentials, controller.signal)
      if (!controller.signal.aborted) { setPassword(''); onLogin(session) }
    } catch (failure) {
      if (!controller.signal.aborted) setError(failure instanceof Error ? failure.message : 'No se pudo iniciar sesión.')
    } finally {
      busy.current = false
      if (!controller.signal.aborted) setLoading(false)
    }
  }
  return <form className="customer-login-form" onSubmit={submit} aria-busy={loading}>
    <p>Iniciá sesión con tu cuenta de cliente.</p>
    <fieldset disabled={loading}>
      <legend className="public-cart-announcement">Inicio de sesión de cliente</legend>
      <label htmlFor={`${id}-email`}>Correo electrónico</label>
      <input id={`${id}-email`} type="email" autoComplete="username" value={email}
        onChange={event => setEmail(event.target.value)} required />
      <label htmlFor={`${id}-password`}>Contraseña</label>
      <input id={`${id}-password`} type="password" autoComplete="current-password" value={password}
        onChange={event => setPassword(event.target.value)} required />
      <button type="submit">{loading ? 'Iniciando sesión…' : 'Iniciar sesión'}</button>
    </fieldset>
    {error && <p className="customer-login-error" role="alert">{error}</p>}
  </form>
}
