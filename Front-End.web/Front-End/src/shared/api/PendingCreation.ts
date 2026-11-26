import { createOperationId } from '../../features/sales/utils/operationId.ts'

type Body<T> = T & { id_operacion: string }
// Conserva la solicitud exacta incluso si se recarga la pestaña tras perder la respuesta.
export class PendingCreation<T extends object> {
  body: Body<T> | null = null
  private storageError = false
  private key: string
  private storage?: Pick<Storage, 'getItem' | 'setItem' | 'removeItem'>
  constructor(key: string, storage?: Pick<Storage, 'getItem' | 'setItem' | 'removeItem'>) {
    this.key = key
    try {
      this.storage = storage ?? globalThis.sessionStorage
      const raw = this.storage?.getItem(key)
      if (raw) {
        const saved = JSON.parse(raw)
        if (!saved || typeof saved !== 'object' || typeof saved.id_operacion !== 'string' || !/^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(saved.id_operacion)) throw new Error('Operación inválida')
        this.body = saved
      }
    } catch { this.storageError = true }
  }
  prepare(payload: T): Body<T> {
    if (this.storageError || !this.storage) throw new Error('No se pudo conservar la operación. Verifica el almacenamiento del navegador antes de registrar.')
    const body: Body<T> = this.body ?? JSON.parse(JSON.stringify({ ...payload, id_operacion: createOperationId() }))
    this.storage.setItem(this.key, JSON.stringify(body))
    this.body = body
    return body
  }
  complete() {
    this.storage?.removeItem(this.key)
    this.body = null
  }
}
