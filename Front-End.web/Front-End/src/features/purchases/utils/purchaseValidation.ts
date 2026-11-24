import type { CreatePurchase, PurchaseLine } from '../types/purchase.ts'

export function amountCents(value: string): number | null {
  const text = value.trim().replace(',', '.')
  if (!/^\d{1,8}(?:\.\d{1,2})?$/.test(text)) return null
  const [whole, decimals = ''] = text.split('.')
  const cents = Number(whole) * 100 + Number(decimals.padEnd(2, '0'))
  return cents <= 9999999999 ? cents : null
}
export function purchaseSupplier(supplier: string, name = ''): Pick<CreatePurchase, 'id_proveedor' | 'nombre_proveedor'> {
  if (supplier) {
    const id = Number(supplier)
    if (!Number.isSafeInteger(id) || id <= 0 || id > 2147483647) throw new Error('Selecciona una distribuidora válida.')
    return { id_proveedor: id }
  }
  const trimmed = name.trim()
  if (!trimmed || name.length > 100 || /[\u0000-\u001f\u007f-\u009f]/.test(name))
    throw new Error('Ingresa el nombre de la distribuidora, de hasta 100 caracteres.')
  return { nombre_proveedor: trimmed }
}
export function buildPurchase(supplier: string, note: string, paid: string, lines: PurchaseLine[], supplierName = ''): CreatePurchase {
  const selectedSupplier = purchaseSupplier(supplier, supplierName)
  if (!lines.length || lines.length > 100) throw new Error('Agrega entre 1 y 100 productos.')
  if (note.trim().length > 100) throw new Error('La observación admite hasta 100 caracteres.')
  const ids = new Set<number>()
  let total = 0
  const details = lines.map(line => {
    if (!Number.isSafeInteger(line.idProducto) || line.idProducto <= 0 || ids.has(line.idProducto))
      throw new Error('Los productos no pueden repetirse.')
    ids.add(line.idProducto)
    if (!/^\d+$/.test(line.cantidad)) throw new Error(`Ingresa unidades enteras para ${line.nombre}.`)
    const quantity = Number(line.cantidad)
    if (!Number.isSafeInteger(quantity) || quantity < 1 || quantity > 2147483647)
      throw new Error(`Revisa la cantidad de ${line.nombre}.`)
    const cents = amountCents(line.total)
    if (cents === null || cents <= 0) throw new Error(`Ingresa el total de ${line.nombre} con hasta 2 decimales.`)
    total += cents
    return { id_producto: line.idProducto, cantidad: quantity, precio: cents / 100 }
  })
  if (total > 9999999999) throw new Error('El total de la compra supera el límite permitido.')
  const payment = amountCents(paid)
  if (payment === null || payment > total) throw new Error('El pago debe tener hasta 2 decimales y no superar el total.')
  return { ...selectedSupplier, observacion: note.trim() || null, monto_pagado: payment / 100, detalle_compra: details }
}
