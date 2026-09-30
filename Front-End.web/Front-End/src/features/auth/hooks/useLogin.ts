import { useEffect, useRef, useState } from 'react'
import { loginStaff } from '../api/authApi'
import { validateLogin } from '../schemas/loginSchema'
import type { LoginErrors } from '../schemas/loginSchema'
import type { LoginCredentials } from '../types/auth'
import { ApiError } from '../../../shared/api/ApiError'
import { useAuth } from './useAuth'

export function useLogin() {
  const { startSession } = useAuth()
  const [errors, setErrors] = useState<LoginErrors>({})
  const [error, setError] = useState('')
  const [pending, setPending] = useState(false)
  const [retryAt, setRetryAt] = useState(0)
  const [secondsRemaining, setSecondsRemaining] = useState(0)
  const activeRequest = useRef<AbortController | null>(null)

  useEffect(() => () => activeRequest.current?.abort(), [])
  useEffect(() => {
    if (!retryAt) return
    const update = () => setSecondsRemaining(Math.max(0, Math.ceil((retryAt - Date.now()) / 1000)))
    update()
    const timer = window.setInterval(update, 1000)
    return () => window.clearInterval(timer)
  }, [retryAt])

  async function submit(credentials: LoginCredentials) {
    if (activeRequest.current || Date.now() < retryAt) return
    const validation = validateLogin(credentials)
    setErrors(validation)
    setError('')
    if (Object.keys(validation).length) return
    const controller = new AbortController()
    activeRequest.current = controller
    setPending(true)
    try {
      const session = await loginStaff(credentials, controller.signal)
      if (!controller.signal.aborted) startSession(session)
    } catch (failure) {
      if (controller.signal.aborted) return
      setError(failure instanceof ApiError ? failure.message : 'No se pudo iniciar sesión.')
      if (failure instanceof ApiError && failure.status === 429) {
        const seconds = failure.retryAfterSeconds ?? 60
        setRetryAt(Date.now() + seconds * 1000)
        setSecondsRemaining(seconds)
      }
    } finally {
      if (activeRequest.current === controller) activeRequest.current = null
      if (!controller.signal.aborted) setPending(false)
    }
  }
  return { submit, errors, error, pending, secondsRemaining }
}