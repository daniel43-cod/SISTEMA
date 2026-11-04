import { useEffect, useState } from 'react'
import type { CashSession, CashSummary } from '../api/cashApi'

const money = (value: number) => new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' }).format(value)
const day = new Intl.DateTimeFormat('en-CA', { timeZone: 'America/Guatemala', year: 'numeric', month: '2-digit', day: '2-digit' })
const dayLabel = new Intl.DateTimeFormat('es-GT', { timeZone: 'America/Guatemala', weekday: 'long', day: 'numeric', month: 'long' })
const time = new Intl.DateTimeFormat('es-GT', { timeZone: 'America/Guatemala', hour: 'numeric', minute: '2-digit' })
const openingDate = new Intl.DateTimeFormat('es-GT', { dateStyle: 'short', timeStyle: 'short' })
const incoming = (nature: string) => ['ENTRADA', 'INGRESO'].includes(nature.trim().toUpperCase())

export function MobileCashOverview({ current, summary, error, saving, onClose, onExpense }: {
  current: CashSession; summary: CashSummary | null; error: string; saving: boolean
  onClose: () => void; onExpense?: () => void
}) {
  const [filter, setFilter] = useState<'all' | 'in' | 'out'>('all')
  const [now, setNow] = useState(() => Date.now())
  useEffect(() => { const timer = window.setInterval(() => setNow(Date.now()), 60000); return () => window.clearInterval(timer) }, [])
  const validSummary = summary?.id_sesion_caja === current.id_sesion_caja ? summary : null
  const movements = [...(validSummary?.movimientos ?? [])].sort((a, b) => Date.parse(b.fecha_movimiento) - Date.parse(a.fecha_movimiento) || b.id_movimiento_caja - a.id_movimiento_caja)
  const visible = movements.filter(m => filter === 'all' || (filter === 'in' ? incoming(m.naturaleza) : !incoming(m.naturaleza)))
  const elapsedHours = Math.max(0, Math.floor((now - Date.parse(current.fecha_apertura)) / 3600000))
  const groups = new Map<string, typeof visible>()
  visible.forEach(movement => {
    const key = day.format(new Date(movement.fecha_movimiento))
    groups.set(key, [...(groups.get(key) ?? []), movement])
  })
  return <div className="cash-mobile-view">
    <section className="cash-mobile-balance" aria-label="Resumen de caja">
      <div className="cash-mobile-balance-heading">
        <div className="cash-mobile-amount-block"><span>Monto esperado en caja</span><strong className="cash-mobile-expected">{!error && validSummary ? money(validSummary.saldo_esperado) : '—'}</strong></div>
        <div className="cash-mobile-opening-person">
          <span>Abierta por</span>
          <strong>{current.usuario_apertura}</strong>
          <time dateTime={current.fecha_apertura}>{openingDate.format(new Date(current.fecha_apertura))}</time>
        </div>
      </div>
      <dl>
        <div><dt>Inicial</dt><dd>{money(current.monto_inicial)}</dd></div>
        <div><dt>Entradas</dt><dd className="cash-incoming">{!error && validSummary ? '+ ' + money(validSummary.total_entradas) : '—'}</dd></div>
        <div><dt>Salidas</dt><dd className="cash-outgoing">{!error && validSummary ? '− ' + money(validSummary.total_salidas) : '—'}</dd></div>
      </dl>
    </section>
    {elapsedHours >= 24 && <p className="cash-mobile-duration">Este turno lleva {Math.floor(elapsedHours / 24)} d {elapsedHours % 24} h abierto.</p>}
    {error ? <p role="alert" className="cash-error">{error} Usa actualizar para volver a consultar.</p> : !validSummary ? <p role="status">Consultando movimientos…</p> : <>
      <div className="cash-mobile-filters" role="group" aria-label="Filtrar movimientos">
        <button type="button" aria-pressed={filter === 'all'} onClick={() => setFilter('all')}>Todos ({movements.length})</button>
        <button type="button" aria-pressed={filter === 'in'} onClick={() => setFilter('in')}>Entradas ({movements.filter(m => incoming(m.naturaleza)).length})</button>
        <button type="button" aria-pressed={filter === 'out'} onClick={() => setFilter('out')}>Salidas ({movements.filter(m => !incoming(m.naturaleza)).length})</button>
      </div>
      <h3>Movimientos del turno ({validSummary.total_movimientos})</h3>
      {validSummary.total_movimientos > movements.length && <p className="cash-mobile-limit">Se muestran los últimos {movements.length} de {validSummary.total_movimientos} movimientos. Los filtros aplican a estos movimientos; los importes del resumen incluyen todo el turno.</p>}
      {!visible.length && <p>No hay movimientos para este filtro.</p>}
      {[...groups].map(([key, items]) => <section key={key} className="cash-mobile-day" aria-label={dayLabel.format(new Date(items[0]!.fecha_movimiento))}>
        <h4>{key === day.format(new Date(now)) ? 'Hoy' : dayLabel.format(new Date(items[0]!.fecha_movimiento))}</h4>
        <ul>{items.map(movement => {
          const entry = incoming(movement.naturaleza)
          return <li key={movement.id_movimiento_caja} className="cash-mobile-movement">
            <span className={entry ? 'cash-mobile-arrow is-in' : 'cash-mobile-arrow is-out'} aria-label={entry ? 'Entrada' : 'Salida'}>{entry ? '↗' : '↙'}</span>
            <div><strong>{movement.tipo_movimiento}</strong><small>{[movement.descripcion, movement.usuario, time.format(new Date(movement.fecha_movimiento))].filter(Boolean).join(' · ')}</small></div>
            <strong className={entry ? 'cash-incoming' : 'cash-outgoing'}>{entry ? '+' : '−'}{money(movement.monto)}</strong>
          </li>
        })}</ul>
      </section>)}
    </>}
    <div className="cash-mobile-actions">
      <button type="button" disabled={saving} onClick={onClose}>Cerrar caja</button>
      {onExpense && <button type="button" disabled={saving} onClick={onExpense}>+ Gasto</button>}
    </div>
  </div>
}