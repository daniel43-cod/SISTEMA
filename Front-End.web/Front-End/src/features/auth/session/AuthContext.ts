import { createContext } from 'react'
import type { StaffSession } from '../types/auth'
export type AuthContextValue = {
  session: StaffSession | null
  notice: string
  startSession: (session: StaffSession) => void
  logout: (notice?: string) => void
}
export const AuthContext = createContext<AuthContextValue | null>(null)