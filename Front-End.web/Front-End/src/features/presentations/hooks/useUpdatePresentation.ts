import { useEffect, useRef, useState } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { updatePresentation } from '../api/presentationsApi'
import { validateDescription } from '../schemas/presentationSchema'

export function useUpdatePresentation() {
  const { session, logout } = useAuth()
  const [pending, setPending] = useState(false)
  const [fieldError, setFieldError] = useState<string>()
  const [error, setError] = useState('')
  const request = useRef<AbortController | null>(null)
  useEffect(() => () => request.current?.abort(), [])

  async function submit(id: number, description: string) {
    if (request.current) return false
    setError('')
    const validation = validateDescription(description)
    setFieldError(validation)
    if (validation) return false
    if (!session || session.expiresAt <= Date.now()) {
      logout('Tu sesión venció. Inicia sesión nuevamente.')
      return false
    }
    if (session.user.role !== 'ADMINISTRADOR') {
      setError('Solo un administrador puede actualizar presentaciones.')
      return false
    }
    const controller = new AbortController()
    request.current = controller
    setPending(true)
    try {
      await updatePresentation(id, { descripcion: description }, session.token, controller.signal)
      return !controller.signal.aborted
    } catch (failure) {
      if (controller.signal.aborted) return false
      if (failure instanceof ApiError && failure.status === 401)
        logout('La sesión ya no es válida. Inicia sesión nuevamente.')
      else setError(failure instanceof ApiError ? failure.message : 'No se pudo actualizar la presentación.')
      return false
    } finally {
      if (request.current === controller) request.current = null
      if (!controller.signal.aborted) setPending(false)
    }
  }
  return { submit, pending, fieldError, error }
}