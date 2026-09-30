export type StaffRole = 'ADMINISTRADOR' | 'VENDEDOR'
export type LoginCredentials = { usuario: string; password: string }
export type StaffSession = {
  user: { id: number; name: string; role: StaffRole }
  token: string
  expiresAt: number
}