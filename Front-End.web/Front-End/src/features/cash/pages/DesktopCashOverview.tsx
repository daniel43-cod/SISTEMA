import { useEffect, useState } from 'react'
import { RefreshButton } from '../../../shared/ui/RefreshButton'
import type { CashSession, CashSummary } from '../api/cashApi'
import { cashAgeHours, cashDate, cashDay, cashIncoming, groupCashMovements } from '../utils/cashMovementView'

const money = (value: number) => new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' }).format(value)
const dayLabel = new Intl.DateTimeFormat('es-GT', { timeZone: 'America/Guatemala', weekday: 'long', day: 'numeric', month: 'long' })
const time = new Intl.DateTimeFormat('es-GT', { timeZone: 'America/Guatemala', hour: 'numeric', minute: '2-digit' })
const fullDate = new Intl.DateTimeFormat('es-GT', { timeZone: 'America/Guatemala', dateStyle: 'short', timeStyle: 'short' })
export function DesktopCashOverview({ current, summary, error, saving, onClose, onRefresh }: {
  current: CashSession; summary: CashSummary | null; error: string; saving: boolean
  onClose: () => void; onRefresh: () => void
}) {
  const [filter, setFilter] = useState<'all' | 'in' | 'out'>('all')
  const [concept, setConcept] = useState('')
  const [now, setNow] = useState(Date.now)
  useEffect(() => { const timer = window.setInterval(() => setNow(Date.now()), 60000); return () => window.clearInterval(timer) }, [])
  const valid = summary?.id_sesion_caja === current.id_sesion_caja ? summary : null
  const movements = valid?.movimientos ?? []
  const aged = cashAgeHours(current.fecha_apertura, now) >= 24
  const days = Math.floor(cashAgeHours(current.fecha_apertura, now) / 24)
  const grouped = aged && new Set(movements.map(m => cashDay(m.fecha_movimiento))).size > 1
  const incomingCount = movements.filter(m => cashIncoming(m.naturaleza)).length
  const concepts = [...new Set(movements.map(m => m.tipo_movimiento))].sort((a, b) => a.localeCompare(b, 'es'))
  const visible = movements.filter(m => (filter === 'all' || (filter === 'in' ? cashIncoming(m.naturaleza) : !cashIncoming(m.naturaleza))) && (!concept || m.tipo_movimiento === concept))
  const partial = !!valid && (valid.total_movimientos > movements.length || filter !== 'all' || !!concept)
  const groups = groupCashMovements(visible, grouped)
  return <div className="cash-desktop-view">
    {aged && <div className="cash-desktop-age" role="status">
      <p><strong>Este turno lleva {days} {days === 1 ? 'día abierto' : 'días abierto'}.</strong> Considera cerrarlo y abrir uno nuevo para mantener separados los turnos.</p>
      <button type="button" disabled={saving} onClick={onClose}>Cerrar caja ahora</button>
    </div>}
    <div className="cash-desktop-equation" aria-label="Resumen del turno">
      <div><span>Monto inicial</span><strong>{money(current.monto_inicial)}</strong></div><span aria-hidden="true">+</span>
      <div><span>Entradas</span><strong className="cash-incoming">{!error && valid ? money(valid.total_entradas) : '—'}</strong></div><span aria-hidden="true">−</span>
      <div><span>Salidas</span><strong className="cash-outgoing">{!error && valid ? money(valid.total_salidas) : '—'}</strong></div><span aria-hidden="true">=</span>
      <div className="cash-desktop-expected"><span>Monto esperado en caja</span><strong>{!error && valid ? money(valid.saldo_esperado) : '—'}</strong></div>
    </div>
    <div className="cash-desktop-opening"><span>Abierta por {current.usuario_apertura} · {fullDate.format(cashDate(current.fecha_apertura))}</span>
      {!aged && <button type="button" disabled={saving} onClick={onClose}>Cerrar caja</button>}
    </div>
    <div className="cash-desktop-movement-toolbar">
      <h3>Movimientos del turno ({valid?.total_movimientos ?? '—'})</h3>
      <div className="cash-desktop-tabs" role="group" aria-label="Filtrar movimientos">
        <button type="button" aria-pressed={filter === 'all'} onClick={() => setFilter('all')}>Todos ({movements.length})</button>
        <button type="button" aria-pressed={filter === 'in'} onClick={() => setFilter('in')}>Entradas ({incomingCount})</button>
        <button type="button" aria-pressed={filter === 'out'} onClick={() => setFilter('out')}>Salidas ({movements.length - incomingCount})</button>
      </div>
      <label>Concepto<select value={concept} onChange={e => setConcept(e.target.value)}><option value="">Todos</option>{concepts.map(c => <option key={c} value={c}>{c}</option>)}</select></label>
      <RefreshButton label="Actualizar movimientos de caja" loading={saving || !valid && !error} onClick={onRefresh} />
    </div>
    {error ? <p role="alert" className="cash-error">{error}</p> : !valid ? <p role="status">Consultando movimientos…</p> : <>
      {valid.total_movimientos > movements.length && <p className="cash-desktop-limit">Se muestran los últimos {movements.length} de {valid.total_movimientos} movimientos. Los filtros y netos por día corresponden a los movimientos mostrados; el resumen incluye todo el turno.</p>}
      {!visible.length ? <p>No hay movimientos para este filtro.</p> : <div className="cash-desktop-table-scroll">
        <table className="cash-desktop-table"><thead><tr><th>Movimiento</th><th>Encargado</th><th>{grouped ? 'Hora' : 'Fecha y hora'}</th><th>Monto</th></tr></thead>
          {groups.map(group => <tbody key={group.key}>
            {grouped && <tr className="cash-desktop-day"><th colSpan={3} scope="rowgroup">{group.key === cashDay(new Date(now).toISOString()) ? 'Hoy' : dayLabel.format(cashDate(group.items[0]!.fecha_movimiento))}</th>
              <td>{partial ? 'Neto mostrado' : 'Neto del día'}: <strong className={group.net >= 0 ? 'cash-incoming' : 'cash-outgoing'}>{group.net >= 0 ? '+' : '−'}{money(Math.abs(group.net))}</strong></td></tr>}
            {group.items.map(m => {
              const entry = cashIncoming(m.naturaleza)
              return <tr key={m.id_movimiento_caja}>
                <td><div className="cash-desktop-movement-label"><span className={entry ? 'cash-desktop-arrow is-in' : 'cash-desktop-arrow is-out'} aria-label={entry ? 'Entrada' : 'Salida'}>{entry ? '↗' : '↙'}</span>
                  <div><strong>{m.tipo_movimiento}</strong>{m.descripcion && <small>{m.descripcion}</small>}</div></div></td>
                <td>{m.usuario}</td><td><time dateTime={m.fecha_movimiento}>{(grouped ? time : fullDate).format(cashDate(m.fecha_movimiento))}</time></td>
                <td><strong className={entry ? 'cash-incoming' : 'cash-outgoing'}>{entry ? '+' : '−'}{money(m.monto)}</strong></td>
              </tr>
            })}
          </tbody>)}
        </table>
      </div>}
    </>}
  </div>
}