import { createExpense } from '../src/features/expenses/api/expensesApi.ts'
import { createSale } from '../src/features/sales/api/salesApi.ts'
import type { CreatePurchase } from '../src/features/purchases/types/purchase.ts'
import test from 'node:test'
import assert from 'node:assert/strict'
import { PendingCreation } from '../src/shared/api/PendingCreation.ts'
import { createPurchase } from '../src/features/purchases/api/purchasesApi.ts'

function storage() {
  const values = new Map<string,string>()
  return { getItem: (k: string) => values.get(k) ?? null, setItem: (k: string,v: string) => { values.set(k,v) }, removeItem: (k: string) => { values.delete(k) } }
}
test('La operación se conserva antes de enviar y se recupera tras recargar', () => {
  const disk = storage()
  const first = new PendingCreation<{ monto: number; observacion: string }>('gasto-1', disk)
  const request = first.prepare({ monto: 10, observacion: 'Original' })
  assert.match(request.id_operacion, /^[0-9a-f-]{36}$/)
  const reloaded = new PendingCreation<{ monto: number; observacion: string }>('gasto-1', disk)
  assert.deepEqual(reloaded.prepare({ monto: 999, observacion: 'Cambio' }), request)
})
test('Confirmar libera la clave y una operación nueva recibe otra', () => {
  const pending = new PendingCreation<{ monto: number }>('gasto', storage())
  const first = pending.prepare({ monto: 10 })
  pending.complete()
  assert.notEqual(pending.prepare({ monto: 10 }).id_operacion, first.id_operacion)
})
test('Conserva una fotografía de los detalles y no referencias editables', () => {
  const pending = new PendingCreation<{ detalles: { cantidad: number }[] }>('compra', storage())
  const draft = { detalles: [{ cantidad: 2 }] }
  pending.prepare(draft); draft.detalles[0].cantidad = 100
  assert.equal(pending.prepare(draft).detalles[0].cantidad, 2)
})
test('Usuarios y operaciones distintas no comparten claves', () => {
  const disk = storage()
  const a = new PendingCreation<{ monto: number }>('gasto-1', disk).prepare({ monto: 10 })
  const b = new PendingCreation<{ monto: number }>('gasto-2', disk).prepare({ monto: 10 })
  assert.notEqual(a.id_operacion, b.id_operacion)
  assert.equal(new PendingCreation('compra-1', disk).body, null)
})
test('Almacenamiento bloqueado impide preparar el envío', () => {
  const disk = storage(); disk.setItem = () => { throw new Error('Bloqueado') }
  const pending = new PendingCreation<{ monto: number }>('gasto', disk)
  assert.throws(() => pending.prepare({ monto: 10 }))
  assert.equal(pending.body, null)
})
test('Solicitud guardada corrupta no se sobrescribe con una operación nueva', () => {
  const disk = storage(); disk.setItem('gasto', '{inválido')
  assert.throws(() => new PendingCreation('gasto', disk).prepare({ monto: 10 }))
  assert.equal(disk.getItem('gasto'), '{inválido')
})
test('Crear compra reenvía la misma clave y el mismo contenido', async t => {
  const pending = new PendingCreation<{ detalle_compra: { id_producto: number; cantidad: number; precio: number }[]; monto_pagado: number; observacion: null }>('compra', storage())
  const body = pending.prepare({ detalle_compra: [{ id_producto: 1, cantidad: 2, precio: 10 }], monto_pagado: 0, observacion: null })
  const requests: unknown[] = []
  t.mock.method(globalThis,'fetch', async (_url: string, options: RequestInit) => {
    requests.push(JSON.parse(String(options.body)))
    return Response.json({ id_compra: 5, total_compra: 10, saldo_pendiente: 10, productos: [] })
  })
  await createPurchase('prueba', body); await createPurchase('prueba', pending.prepare(body))
  assert.deepEqual(requests[0],requests[1])
  assert.equal((requests[0] as { id_operacion: string }).id_operacion,body.id_operacion)
})

for (const tipo of ['venta','compra','gasto']) test('Respuesta perdida y recarga: reintenta la misma ' + tipo, async t => {
  const disk = storage()
  const pending = new PendingCreation<Record<string,unknown>>(tipo, disk)
  const draft = { descripcion: 'Original', monto: 10, observacion: null, monto_pagado: 0, detalle_compra: [{ id_producto: 1, cantidad: 1, precio: 10 }] }
  const original = pending.prepare(draft)
  const registrados = new Map<string,unknown>(); let llamadas = 0
  t.mock.method(globalThis,'fetch',async (_url: string, options: RequestInit) => {
    const body = JSON.parse(String(options.body)); llamadas++
    if (!registrados.has(body.id_operacion)) registrados.set(body.id_operacion,body)
    if (llamadas === 1) throw new TypeError('Respuesta perdida después de guardar')
    return Response.json({ id_venta: 7, id_compra: 7, id_gasto: 7, total: 10, total_compra: 10, saldo_pendiente: 10, productos: [] })
  })
  const send = (body: Record<string,unknown> & { id_operacion: string }) => tipo === 'venta' ? createSale(body,'prueba') : tipo === 'compra'
    ? createPurchase('prueba',body as unknown as CreatePurchase & { id_operacion: string })
    : createExpense('prueba',body as unknown as { descripcion: string; monto: number; observacion: null; id_operacion: string })
  await assert.rejects(send(original))
  const reloaded = new PendingCreation<Record<string,unknown>>(tipo,disk)
  await send(reloaded.prepare({ ...draft, monto: 999 }))
  assert.equal(llamadas,2); assert.equal(registrados.size,1)
  assert.deepEqual(registrados.get(original.id_operacion),original)
  reloaded.complete(); assert.equal(new PendingCreation(tipo,disk).body,null)
})