import type { PurchaseLine } from '../types/purchase'
import { amountCents } from '../utils/purchaseValidation'

const costFormat = new Intl.NumberFormat('es-GT', {
  style: 'currency', currency: 'GTQ', minimumFractionDigits: 2, maximumFractionDigits: 4,
})

// Referencia visual: no modifica los importes enviados al backend.
export function PurchaseUnitCost({ line }: { line: PurchaseLine }) {
  const units = Number(line.cantidad)
  const cents = amountCents(line.total)
  const valid = /^\d+$/.test(line.cantidad) && Number.isSafeInteger(units) && units > 0 && units <= 2147483647 && cents !== null && cents > 0
  return <output aria-label="Costo unitario estimado">{valid ? costFormat.format(cents / 100 / units) : '—'}</output>
}