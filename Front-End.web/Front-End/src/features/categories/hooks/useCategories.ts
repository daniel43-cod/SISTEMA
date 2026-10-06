import { useCallback, useEffect, useState } from 'react'
import { useAuth } from '../../auth'
import { listCategories } from '../api/categoriesApi'
import { ApiError } from '../../../shared/api/ApiError'
import type { CategoryResponse } from '../types/category'

export function useCategories(administration = false) {
  const { session, logout } = useAuth()
  const [items, setItems] = useState<CategoryResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [revision, setRevision] = useState(0)
  const reload = useCallback(() => setRevision(value => value + 1), [])
  useEffect(() => {
    if (!session) return
    const controller = new AbortController()
    async function load() {
      setLoading(true); setError('')
      try {
        const rows = await listCategories(session!.token, controller.signal, administration)
        if (!controller.signal.aborted) setItems(rows)
      } catch (failure) {
        if (controller.signal.aborted) return
        if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
        else setError(failure instanceof ApiError ? failure.message : 'No se pudieron cargar las categorías.')
      } finally { if (!controller.signal.aborted) setLoading(false) }
    }
    void load()
    return () => controller.abort()
  }, [session, logout, revision, administration])
  return { items, loading, error, reload }
}

