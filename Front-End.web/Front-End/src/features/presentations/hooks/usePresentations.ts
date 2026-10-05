import { useCallback, useEffect, useState } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { listPresentations } from '../api/presentationsApi'
import type { PresentationResponse } from '../types/presentation'

type Result = { revision: number; token: string; administration: boolean; items: PresentationResponse[]; error: string }
export function usePresentations(administration = false) {
  const { session, logout } = useAuth()
  const [result, setResult] = useState<Result | null>(null)
  const [revision, setRevision] = useState(0)
  const reload = useCallback(() => setRevision(value => value + 1), [])
  const token = session?.token
  const expiresAt = session?.expiresAt
  const current = result?.token === token && result?.revision === revision && result.administration === administration ? result : null

  useEffect(() => {
    const controller = new AbortController()
    async function load() {
      if (!token || !expiresAt || expiresAt <= Date.now()) {
        logout('Tu sesión venció. Inicia sesión nuevamente.')
        return
      }
      try {
        const items = await listPresentations(token, controller.signal, administration)
        if (!controller.signal.aborted) setResult({ revision, token, administration, items, error: '' })
      } catch (failure) {
        if (controller.signal.aborted) return
        if (failure instanceof ApiError && failure.status === 401)
          logout('La sesión ya no es válida. Inicia sesión nuevamente.')
        else setResult({ revision, token, administration, items: [], error: failure instanceof ApiError ? failure.message : 'No se pudo cargar el listado.' })
      }
    }
    void load()
    return () => controller.abort()
  }, [token, expiresAt, logout, revision, administration])

  return { items: current?.items ?? [], loading: current === null, error: current?.error ?? '', reload }
}