import { useCallback, useEffect, useState } from 'react'
import { ApiError } from '../../../shared/api/ApiError'
import { listPublicBrands } from '../api/publicCatalogApi'
import type { PublicBrand } from '../types/brand'

export function usePublicBrands(idCategoria: number) {
  const [items, setItems] = useState<PublicBrand[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [revision, setRevision] = useState(0)
  const reload = useCallback(() => setRevision(value => value + 1), [])

  useEffect(() => {
    const controller = new AbortController()
    async function load() {
      setLoading(true)
      setError('')
      setItems([])
      try {
        const brands = await listPublicBrands(idCategoria, controller.signal)
        if (!controller.signal.aborted) setItems(brands)
      } catch (failure) {
        if (!controller.signal.aborted)
          setError(failure instanceof ApiError ? failure.message : 'No se pudieron cargar las marcas.')
      } finally {
        if (!controller.signal.aborted) setLoading(false)
      }
    }
    void load()
    return () => controller.abort()
  }, [idCategoria, revision])

  return { items, loading, error, reload }
}
