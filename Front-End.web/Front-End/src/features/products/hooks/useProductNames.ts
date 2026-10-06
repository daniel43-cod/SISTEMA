import { useEffect, useState } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { loadProductNames } from '../api/searchProducts'
import type { ProductSuggestion } from '../api/searchProducts'

export function useProductNames(revision: number) {
  const { session, logout } = useAuth()
  const token = session?.user.role === 'ADMINISTRADOR' ? session.token : undefined
  const [result, setResult] = useState<{ token: string; revision: number; items: ProductSuggestion[]; error: string } | null>(null)
  const current = result && result.token === token && result.revision === revision ? result : null
  useEffect(() => {
    const controller = new AbortController()
    if (!token) return
    void loadProductNames(token, controller.signal).then(items => {
      if (!controller.signal.aborted) setResult({ token, revision, items, error: '' })
    }).catch(failure => {
      if (controller.signal.aborted) return
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else setResult({ token, revision, items: [], error: failure instanceof ApiError ? failure.message : 'No se pudieron cargar los nombres.' })
    })
    return () => controller.abort()
  }, [token, revision, logout])
  return { items: current?.items ?? [], error: current?.error ?? '', loading: Boolean(token) && !current }
}