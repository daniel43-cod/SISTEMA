import { useCallback, useEffect, useState } from 'react'
import { ApiError } from '../../../shared/api/ApiError'
import { listPublicCategories } from '../api/publicCatalogApi'
import type { PublicCategory } from '../types/category'

export function usePublicCategories() {
  const [items, setItems] = useState<PublicCategory[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [revision, setRevision] = useState(0)
  const reload = useCallback(() => setRevision(value => value + 1), [])

  useEffect(() => {
    const controller = new AbortController()
    async function load() {
      setLoading(true)
      setError('')
      try {
        const categories = await listPublicCategories(controller.signal)
        if (!controller.signal.aborted) setItems(categories)
      } catch (failure) {
        if (!controller.signal.aborted)
          setError(failure instanceof ApiError ? failure.message : 'No se pudieron cargar las categorías.')
      } finally {
        if (!controller.signal.aborted) setLoading(false)
      }
    }
    void load()
    return () => controller.abort()
  }, [revision])

  return { items, loading, error, reload }
}
