import { requestJson } from '../../../shared/api/httpClient'
import { useCallback, useEffect, useRef, useState } from 'react'
import type { ReactNode } from 'react'
import { AuthContext } from './AuthContext'
import type { StaffSession } from '../types/auth'

//mantienen la sesion para compartur con los componentes
export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<StaffSession | null>(null)
  const [notice, setNotice] = useState('')
  const currentSession = useRef<StaffSession | null>(null)
  const logout = useCallback((message = '') => {
    const previous = currentSession.current
    currentSession.current = null
    if (!message && previous && previous.expiresAt > Date.now()) {
      void requestJson('/Login/cerrar-sesion', { method: 'POST', token: previous.token }).catch(() => {
        if (!currentSession.current)
          setNotice('Cerraste la sesión en este dispositivo, pero no se pudo confirmar el registro del cierre en el servidor.')
      })
    }
    setSession(null)
    setNotice(message)
  }, [])
  const startSession = useCallback((next: StaffSession) => {
    if (next.expiresAt <= Date.now()) {
      logout('Tu sesión venció. Inicia sesión nuevamente.')
      return
    }
    setNotice('')
    currentSession.current = next
    setSession(next)
  }, [logout])

  useEffect(() => {
    if (!session) return
    const checkExpiration = () => {
      if (Date.now() >= session.expiresAt) logout('Tu sesión venció. Inicia sesión nuevamente.')
    }
    const timer = window.setTimeout(checkExpiration, Math.max(0, session.expiresAt - Date.now()))
    window.addEventListener('focus', checkExpiration)
    document.addEventListener('visibilitychange', checkExpiration)
    return () => {
      window.clearTimeout(timer)
      window.removeEventListener('focus', checkExpiration)
      document.removeEventListener('visibilitychange', checkExpiration)
    }
  }, [session, logout])

  return <AuthContext.Provider value={{ session, notice, startSession, logout }}>{children}</AuthContext.Provider>
}
