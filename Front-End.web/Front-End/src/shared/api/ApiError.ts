//clase para representa errores en las peticiones de la api
export class ApiError extends Error {
  readonly status: number
  readonly retryAfterSeconds: number | null
  constructor(message: string, status = 0, retryAfterSeconds: number | null = null) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.retryAfterSeconds = retryAfterSeconds
  }
}