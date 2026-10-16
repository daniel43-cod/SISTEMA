export interface CustomerSession {
  idCuentaCliente: number
  correoElectronico: string
  token: string
  expiresAt: number
}
export interface CustomerCredentials { correoElectronico: string; password: string }
