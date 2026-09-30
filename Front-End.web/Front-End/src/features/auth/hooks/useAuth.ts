//permite que un componente accesa a la sesion y sus funciones

import { useContext } from 'react'
import { AuthContext } from '../session/AuthContext'
export function useAuth() {
  const auth = useContext(AuthContext)
  if (!auth) throw new Error('useAuth requiere AuthProvider.')
  return auth
}