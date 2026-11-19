import type { CashMovement } from '../api/cashApi.ts'
const localDay = new Intl.DateTimeFormat('en-CA', { timeZone: 'America/Guatemala', year: 'numeric', month: '2-digit', day: '2-digit' })
export function cashDate(value: string): Date {
  return new Date(/(?:Z|[+-]\d{2}:\d{2})$/i.test(value) ? value : value + '-06:00')
}
export function cashDay(value: string): string { return localDay.format(cashDate(value)) }
export function cashIncoming(value: string): boolean { return ['ENTRADA', 'INGRESO'].includes(value.trim().toUpperCase()) }
export function cashAgeHours(opening: string, now: number): number { return Math.max(0, Math.floor((now - cashDate(opening).getTime()) / 3600000)) }
export function groupCashMovements(movements: CashMovement[], groupByDay: boolean) {
  const ordered = [...movements].sort((a, b) => cashDate(b.fecha_movimiento).getTime() - cashDate(a.fecha_movimiento).getTime() || b.id_movimiento_caja - a.id_movimiento_caja)
  const groups = new Map<string, CashMovement[]>()
  for (const movement of ordered) {
    const key = groupByDay ? cashDay(movement.fecha_movimiento) : 'all'
    const rows = groups.get(key) ?? []
    rows.push(movement); groups.set(key, rows)
  }
  return [...groups].map(([key, items]) => ({ key, items,
    net: items.reduce((cents, m) => cents + (cashIncoming(m.naturaleza) ? 1 : -1) * Math.round(m.monto * 100), 0) / 100 }))
}