import { useEffect, useRef, useState } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { createPresentation } from '../api/presentationsApi'
import { validateDescription } from '../schemas/presentationSchema'

export function useCreatePresentation() {
  const { session, logout } = useAuth()
  const [pending, setPending] = useState(false)
  const [fieldError, setFieldError] = useState<string>()
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const activeRequest = useRef<AbortController | null>(null)
  useEffect(() => () => activeRequest.current?.abort(), [])

  async function submit(description: string): Promise<boolean> {
    if (activeRequest.current) return false
    setSuccess('')
    setError('')
    const validation = validateDescription(description)
    setFieldError(validation)
    if (validation) return false
    if (!session || session.expiresAt <= Date.now()) {
      logout('Tu sesión venció. Inicia sesión nuevamente.')
      return false
    }
    if (session.user.role !== 'ADMINISTRADOR') {
      setError('Solo un administrador puede crear presentaciones.')
      return false
    }
    const controller = new AbortController()
    activeRequest.current = controller
    setPending(true)
    try {
      const created = await createPresentation({ descripcion: description }, session.token, controller.signal)
      if (controller.signal.aborted) return false
      setSuccess(`Presentación «${created.descripcion}» creada correctamente.`)
      return true
    } catch (failure) {
      if (controller.signal.aborted) return false
      if (failure instanceof ApiError && failure.status === 401)
        logout('La sesión ya no es válida. Inicia sesión nuevamente.')
      else setError(failure instanceof ApiError ? failure.message : 'No se pudo crear la presentación.')
      return false
    } finally {
      if (activeRequest.current === controller) activeRequest.current = null
      if (!controller.signal.aborted) setPending(false)
    }
  }
  return { submit, pending, fieldError, error, success }
}