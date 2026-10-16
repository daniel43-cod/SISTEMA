import { useCallback, useEffect, useState } from 'react'
import { parseCustomerSession } from '../schemas/customerSession'
import type { CustomerSession } from '../types/customerAuth'

const key = 'sistema.customer.session.v1'
export function useCustomerSession() {
  const [session, setSession] = useState<CustomerSession | null>(() => {
    try { return parseCustomerSession(JSON.parse(sessionStorage.getItem(key) ?? 'null')) }
    catch { return null }
  })
  const [notice, setNotice] = useState('')
  const logout = useCallback(() => {
    setSession(null); setNotice('')
    try { sessionStorage.removeItem(key) } catch { /* La sesión en memoria ya se eliminó. */ }
  }, [])
  const login = useCallback((next: CustomerSession) => {
    setSession(next); setNotice('')
    try { sessionStorage.setItem(key, JSON.stringify(next)) }
    catch { setNotice('La sesión funciona, pero tendrás que iniciar sesión de nuevo si recargas la página.') }
  }, [])
  useEffect(() => {
    if (!session) return
    // Un intervalo evita superar el máximo permitido por setTimeout con tokens de larga duración.
    const expire = () => { if (session.expiresAt <= Date.now()) logout() }
    const interval = window.setInterval(expire, 1000)
    window.addEventListener('focus', expire)
    return () => { window.clearInterval(interval); window.removeEventListener('focus', expire) }
  }, [session, logout])
  return { session, login, logout, notice }
}
