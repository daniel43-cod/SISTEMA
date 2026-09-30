import { useCallback, useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import { AuthContext } from './AuthContext'
import type { StaffSession } from '../types/auth'

//mantienen la sesion para compartur con los componentes
export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<StaffSession | null>(null)
  const [notice, setNotice] = useState('')
  const logout = useCallback((message = '') => {
    setSession(null)
    setNotice(message)
  }, [])
  const startSession = useCallback((next: StaffSession) => {
    if (next.expiresAt <= Date.now()) {
      logout('Tu sesión venció. Inicia sesión nuevamente.')
      return
    }
    setNotice('')
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