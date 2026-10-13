import { useCallback, useEffect, useState } from 'react'
import { ApiError } from '../../../shared/api/ApiError'
import { listPublicProducts } from '../api/publicProductsApi'
import type { PublicProduct } from '../types/product'

export function usePublicProducts(idMarca: number) {
  const [items, setItems] = useState<PublicProduct[]>([])
  const [pagina, setPagina] = useState(1)
  const [hayMas, setHayMas] = useState(false)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [revision, setRevision] = useState(0)
  const reload = useCallback(() => setRevision(value => value + 1), [])
  useEffect(() => {
    const controller = new AbortController()
    async function load() {
      setLoading(true); setError(''); setItems([]); setHayMas(false)
      try {
        const result = await listPublicProducts(idMarca, pagina, controller.signal)
        if (!controller.signal.aborted) { setItems(result.productos); setHayMas(result.hayMas) }
      } catch (failure) {
        if (!controller.signal.aborted)
          setError(failure instanceof ApiError ? failure.message : 'No se pudieron cargar los productos.')
      } finally { if (!controller.signal.aborted) setLoading(false) }
    }
    void load()
    return () => controller.abort()
  }, [idMarca, pagina, revision])
  return { items, pagina, hayMas, loading, error, reload, setPagina }
}
