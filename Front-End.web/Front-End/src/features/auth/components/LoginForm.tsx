//formulario principal
import { useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { Button } from '../../../shared/ui/Button'
import { Input } from '../../../shared/ui/Input'
import { useLogin } from '../hooks/useLogin'
import { useAuth } from '../hooks/useAuth'

export function LoginForm() {
  const [showPassword, setShowPassword] = useState(false)
  const { submit, errors, error, pending, secondsRemaining } = useLogin()
  const { notice } = useAuth()
  const formRef = useRef<HTMLFormElement>(null)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const form = event.currentTarget
    const data = new FormData(form)
    await submit({ usuario: String(data.get('usuario') ?? ''), password: String(data.get('password') ?? '') })
    if (formRef.current) {
      const firstInvalid = formRef.current.querySelector<HTMLInputElement>('[aria-invalid="true"]')
      firstInvalid?.focus()
    }
  }

  return (
    <form ref={formRef} className="login-form" onSubmit={handleSubmit} noValidate aria-busy={pending}>
      {notice && <p className="login-message" role="status">{notice}</p>}
      <Input label="Usuario" name="usuario" autoComplete="username" disabled={pending}
        autoCapitalize="none" spellCheck={false} placeholder="Tu nombre de usuario"
        required maxLength={50} error={errors.usuario} />
      <div className="password-field">
        <Input label="Contraseña" name="password" type={showPassword ? 'text' : 'password'}
          autoComplete="current-password" placeholder="Tu contraseña" required disabled={pending} error={errors.password} />
        <button className="password-toggle" type="button" aria-pressed={showPassword}
          aria-label={showPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'}
          onClick={() => setShowPassword(!showPassword)}>
          <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor"
            strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12Z" />
            <circle cx="12" cy="12" r="3" />
            {showPassword && <path d="m3 3 18 18" />}
          </svg>
        </button>
      </div>
      {error && <p className="login-error" role="alert">{error}</p>}
      <Button type="submit" disabled={pending || secondsRemaining > 0}>
        {pending ? 'Ingresando…' : secondsRemaining > 0 ? `Reintentar en ${secondsRemaining} s` : 'Iniciar sesión'}
      </Button>
      <span className="sr-only" role="status">{pending ? 'Validando tus credenciales.' : ''}</span>
    </form>
  )
}
