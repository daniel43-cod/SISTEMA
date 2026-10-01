import { useCallback, useEffect, useState } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { listBrands } from '../api/brandsApi'
import type { BrandResponse } from '../types/brand'

// Centraliza carga, errores y actualización del listado sin mezclarlos con la interfaz.
export function useBrands() {
  const { session, logout } = useAuth()
  const [items, setItems] = useState<BrandResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [revision, setRevision] = useState(0)
  const reload = useCallback(() => setRevision(value => value + 1), [])
  useEffect(() => {
    if (!session) return
    const controller = new AbortController()
    const token = session.token
    async function load() {
      setLoading(true); setError('')
      try {
        const rows = await listBrands(token, controller.signal)
        if (!controller.signal.aborted) setItems(rows)
      } catch (failure) {
        if (controller.signal.aborted) return
        if (failure instanceof ApiError && failure.status === 401)
          logout('La sesión ya no es válida. Inicia sesión nuevamente.')
        else setError(failure instanceof ApiError ? failure.message : 'No se pudieron cargar las marcas.')
      } finally { if (!controller.signal.aborted) setLoading(false) }
    }
    void load()
    // Cancela la consulta al salir o recargar, evitando resultados de peticiones anteriores.
    return () => controller.abort()
  }, [session, logout, revision])
  return { items, loading, error, reload }
}
