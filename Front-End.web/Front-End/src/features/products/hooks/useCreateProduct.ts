import { useEffect, useRef, useState } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { createProduct } from '../api/productsApi'
import type { CreateProductRequest } from '../types/product'

export function useCreateProduct() {
  const { session, logout } = useAuth()
  const [pending, setPending] = useState(false)
  const [error, setError] = useState('')
  const request = useRef<AbortController | null>(null)
  useEffect(() => () => request.current?.abort(), [])
  async function submit(data: CreateProductRequest) {
    if (request.current) return false
    setError('')
    if (!session || session.expiresAt <= Date.now()) { logout('La sesión venció.'); return false }
    if (session.user.role !== 'ADMINISTRADOR') { setError('No tienes permiso para guardar productos.'); return false }
    const controller = new AbortController()
    request.current = controller
    setPending(true)
    try {
      await createProduct(data, session.token, controller.signal)
      return !controller.signal.aborted
    } catch (failure) {
      if (controller.signal.aborted) return false
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else setError(failure instanceof ApiError ? failure.message : 'No se pudo guardar el producto.')
      return false
    } finally {
      if (request.current === controller) request.current = null
      if (!controller.signal.aborted) setPending(false)
    }
  }
  return { submit, pending, error }
}


