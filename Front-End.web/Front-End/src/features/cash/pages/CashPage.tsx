import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { useAuth } from '../../auth'
import { Input } from '../../../shared/ui/Input'
import { Button } from '../../../shared/ui/Button'
import { RefreshButton } from '../../../shared/ui/RefreshButton'
import { ApiError } from '../../../shared/api/ApiError'
import { loadCash, loadCashSummary, openCash, closeCash, parseOpeningAmount } from '../api/cashApi'
import type { AvailableCash, CashSession, CashClosing, CashSummary } from '../api/cashApi'
import './CashPage.css'

export function CashPage() {
  const { session } = useAuth()
  const [current, setCurrent] = useState<CashSession | null>(null)
  const [summary, setSummary] = useState<CashSummary | null>(null)
  const [summaryError, setSummaryError] = useState('')
  const [closing, setClosing] = useState(false)
  const [counted, setCounted] = useState('')
  const [closingNote, setClosingNote] = useState('')
  const [review, setReview] = useState<{ id: number; amount: number; note: string } | null>(null)
  const [result, setResult] = useState<CashClosing | null>(null)
  const money = (value: number) => new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' }).format(value)
  const [available, setAvailable] = useState<AvailableCash[]>([])
  const [selected, setSelected] = useState('')
  const [amount, setAmount] = useState('')
  const [note, setNote] = useState('')
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [phase, setPhase] = useState<'loading' | 'ready' | 'error'>('loading')
  const [saving, setSaving] = useState(false)
  const [version, setVersion] = useState(0)
  const busy = useRef(false)
  const token = session?.token
  const isAdmin = session?.user.role === 'ADMINISTRADOR'

  useEffect(() => {
    if (!token || !isAdmin) return
    const controller = new AbortController()
    loadCash(token, controller.signal).then(data => {
      if (controller.signal.aborted) return
      setClosing(false)
      setReview(null)
      setCurrent(data.current)
      setAvailable(data.available)
      setSelected(value => data.available.some(item => String(item.idCaja) === value) ? value :
        data.available.length === 1 ? String(data.available[0].idCaja) : '')
      setPhase('ready')
    }).catch(cause => {
      if (controller.signal.aborted) return
      setError(cause instanceof ApiError ? cause.message : 'No se pudo consultar la caja.')
      setPhase('error')
    })
    return () => controller.abort()
  }, [token, isAdmin, version])

  const currentId = current?.id_sesion_caja
  useEffect(() => {
    if (!token || !isAdmin || !currentId || phase !== 'ready') return
    const controller = new AbortController()
    // La consulta es independiente: un fallo del resumen no modifica el estado real de la caja.
    loadCashSummary(token, currentId, controller.signal).then(data => {
      if (!controller.signal.aborted) { setSummary(data); setSummaryError('') }
    }).catch(cause => {
      if (!controller.signal.aborted) setSummaryError(cause instanceof ApiError ? cause.message : 'No se pudieron consultar los movimientos.')
    })
    return () => controller.abort()
  }, [token, isAdmin, currentId, phase, version])

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!token || !isAdmin || busy.current || phase !== 'ready' || current) return
    const parsedAmount = parseOpeningAmount(amount)
    const id = Number(selected)
    setError('')
    setNotice('')
    if (!available.some(item => item.idCaja === id)) { setError('Selecciona una caja.'); return }
    if (parsedAmount === null) { setError('Ingresa un monto de 0 a 99,999,999.99, con hasta dos decimales.'); return }
    if (note.length > 100) { setError('La observación admite hasta 100 caracteres.'); return }
    busy.current = true
    setSaving(true)
    try {
      await openCash(token, id, parsedAmount, note)
      setResult(null)
      setNotice('Caja abierta correctamente.')
      setAmount('')
      setNote('')
    } catch (cause) {
      setError(cause instanceof ApiError && cause.status === 409
        ? 'El estado de la caja cambió. Revisa la información actualizada.'
        : cause instanceof ApiError ? cause.message : 'No se pudo abrir la caja.')
    } finally {
      // Siempre consulta el estado real: un timeout no prueba que la apertura haya fallado.
      setSummary(null); setSummaryError(''); setPhase('loading')
      setVersion(value => value + 1)
      busy.current = false
      setSaving(false)
    }
  }

  function reviewClosing(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!current || saving || busy.current || phase !== 'ready') return
    setError('')
    const parsed = parseOpeningAmount(counted)
    if (parsed === null) { setError('Ingresa el efectivo contado, con hasta dos decimales.'); return }
    if (closingNote.length > 100) { setError('La observación admite hasta 100 caracteres.'); return }
    setReview({ id: current.id_sesion_caja, amount: parsed, note: closingNote })
  }

  async function confirmClosing() {
    if (!token || !isAdmin || !current || !review || busy.current || phase !== 'ready') return
    if (review.id !== current.id_sesion_caja) { setReview(null); setError('El turno cambió. Actualiza la caja.'); return }
    busy.current = true
    setSaving(true)
    setError('')
    setNotice('')
    setResult(null)
    try {
      const closed = await closeCash(token, review.id, review.amount, review.note)
      setResult(closed)
      setNotice('Caja cerrada correctamente.')
      setCounted('')
      setClosingNote('')
    } catch (cause) {
      setError(cause instanceof ApiError && cause.status === 409
        ? 'Otra operación cambió la caja. Revisa el estado actualizado antes de continuar.'
        : cause instanceof ApiError ? cause.message : 'No se pudo cerrar la caja.')
    } finally {
      // No se reintenta automaticamente: el servidor podria haber confirmado el cierre.
      setReview(null)
      setClosing(false)
      setSummary(null); setSummaryError(''); setPhase('loading')
      setVersion(value => value + 1)
      busy.current = false
      setSaving(false)
    }
  }

  if (!isAdmin) return <p>No tienes permiso para administrar la caja.</p>
  return <section className="cash-panel" aria-labelledby="cash-title" aria-busy={phase === 'loading' || saving}>
    <div className="cash-heading">
      <h2 id="cash-title">Sesión de caja</h2>
      <RefreshButton label="Actualizar estado de caja" loading={saving || phase === 'loading'} onClick={() => { setError(''); setSummary(null); setSummaryError(''); setPhase('loading'); setVersion(value => value + 1) }} />
    </div>
    {notice && <p className="cash-success" role="status">{notice}</p>}
    {result && <section className="cash-closing-result" aria-labelledby="cash-result-title">
      <h3 id="cash-result-title">Resultado del cierre · turno {result.id_sesion_caja}</h3>
      <dl className="cash-summary">
        <div><dt>Monto esperado</dt><dd>{money(result.monto_esperado)}</dd></div>
        <div><dt>Efectivo contado</dt><dd>{money(result.monto_contado)}</dd></div>
        <div><dt>{result.diferencia === 0 ? 'Diferencia' : result.diferencia > 0 ? 'Sobrante' : 'Faltante'}</dt>
          <dd className={result.diferencia === 0 ? 'cash-success' : 'cash-difference'}>{money(Math.abs(result.diferencia))}</dd></div>
        <div><dt>Fecha de cierre</dt><dd>{new Date(result.fecha_cierre).toLocaleString('es-GT')}</dd></div>
      </dl>
    </section>}
    {error && <p className="cash-error" role="alert">{error}</p>}
    {phase === 'loading' ? <p role="status">Consultando caja…</p> : phase === 'error' ?
      <Button onClick={() => { setError(''); setSummary(null); setSummaryError(''); setPhase('loading'); setVersion(value => value + 1) }}>Volver a consultar</Button> :
      current ? <>
        <p className="cash-status is-open"><span aria-hidden="true">●</span> Caja abierta</p>
        <dl className="cash-summary">
          <div><dt>Caja</dt><dd>{available.find(item => item.idCaja === current.id_caja)?.nombre ?? 'Caja del turno actual'}</dd></div>
          <div><dt>Monto inicial</dt><dd>{new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' }).format(current.monto_inicial)}</dd></div>
          <div><dt>Abierta por</dt><dd>{current.usuario_apertura}</dd></div>
          <div><dt>Fecha de apertura</dt><dd>{new Date(current.fecha_apertura).toLocaleString('es-GT')}</dd></div>
        </dl>
        <p className="cash-hint">Esta caja está disponible para las operaciones del equipo.</p>
        <section className="cash-movements" aria-labelledby="cash-movements-title">
          <h3 id="cash-movements-title">Movimientos del turno</h3>
          {summaryError ? <p className="cash-error" role="alert">{summaryError} Usa actualizar para volver a consultar.</p> :
            summary?.id_sesion_caja !== current.id_sesion_caja ? <p role="status">Consultando movimientos…</p> : <>
              <dl className="cash-summary">
                <div><dt>Entradas</dt><dd className="cash-incoming">{money(summary.total_entradas)}</dd></div>
                <div><dt>Salidas</dt><dd className="cash-outgoing">{money(summary.total_salidas)}</dd></div>
                <div><dt>Monto esperado</dt><dd>{money(summary.saldo_esperado)}</dd></div>
                <div><dt>Total de movimientos</dt><dd>{summary.total_movimientos}</dd></div>
              </dl>
              <p className="cash-hint">Totales de todo el turno al consultar. Mostrando {summary.movimientos.length} de {summary.total_movimientos} movimientos, del más reciente al más antiguo.</p>
              {summary.movimientos.length === 0 ? <p>Aún no hay movimientos en este turno.</p> :
                <ul className="cash-movement-list">
                  {summary.movimientos.map(movement => {
                    const incoming = ['ENTRADA', 'INGRESO'].includes(movement.naturaleza.trim().toUpperCase())
                    return <li key={movement.id_movimiento_caja}>
                      <div className="cash-movement-info"><strong>{movement.tipo_movimiento}</strong>
                        {movement.descripcion && <span>{movement.descripcion}</span>}
                        <span>{movement.usuario} · {new Date(movement.fecha_movimiento).toLocaleString('es-GT')}</span>
                      </div>
                      <div className={incoming ? 'cash-incoming' : 'cash-outgoing'}>
                        <span>{incoming ? 'Entrada' : 'Salida'}</span><strong>{incoming ? '+' : '−'}{money(movement.monto)}</strong>
                      </div>
                    </li>
                  })}
                </ul>}
            </>}
        </section>
        {!closing ? <Button className="cash-close-button" onClick={() => { setClosing(true); setReview(null); setError(''); setNotice('') }}>Cerrar caja</Button> :
          <form className="cash-form cash-closing-form" onSubmit={reviewClosing}>
            <h3>Cierre del turno</h3>
            <p className="cash-hint">Cuenta el efectivo disponible. Al confirmar, se cerrará la caja para todo el equipo.</p>
            <fieldset disabled={saving || review !== null}>
              <Input label="Efectivo contado (Q)" value={counted} required inputMode="decimal" maxLength={11}
                placeholder="0.00" onChange={event => setCounted(event.target.value)} />
              <Input label="Observación del cierre (opcional)" value={closingNote} maxLength={100}
                onChange={event => setClosingNote(event.target.value)} />
              {!review && <div className="cash-actions">
                <Button type="submit">Revisar cierre</Button>
                <Button className="cash-secondary" onClick={() => { setClosing(false); setError('') }}>Cancelar</Button>
              </div>}
            </fieldset>
            {review && <div className="cash-confirmation" role="group" aria-labelledby="cash-confirm-title">
              <h3 id="cash-confirm-title">Confirmar cierre</h3>
              <p>Efectivo contado: <strong>{money(review.amount)}</strong>.</p>
              <p>Se cerrará el turno {review.id}. El servidor calculará el monto esperado y la diferencia al confirmar.</p>
              <div className="cash-actions">
                <Button className="cash-close-button" disabled={saving} onClick={confirmClosing}>{saving ? 'Cerrando caja…' : 'Confirmar cierre'}</Button>
                <Button className="cash-secondary" disabled={saving} onClick={() => setReview(null)}>Volver a editar</Button>
              </div>
            </div>}
          </form>}
      </> : <>
        <p className="cash-status is-closed">Caja cerrada</p>
        {available.length === 0 ? <p>No hay cajas activas disponibles para abrir.</p> :
          <form className="cash-form" onSubmit={submit}>
            <p className="cash-hint">Abre el turno con el efectivo inicial disponible en caja.</p>
            <fieldset disabled={saving}>
              <div className="field"><label htmlFor="opening-cash">Caja</label>
                <select className="input" id="opening-cash" value={selected} required onChange={event => setSelected(event.target.value)}>
                  <option value="" disabled>Selecciona una caja</option>
                  {available.map(item => <option key={item.idCaja} value={item.idCaja}>{item.nombre}</option>)}
                </select>
              </div>
              <Input label="Monto inicial (Q)" inputMode="decimal" value={amount} required maxLength={11}
                placeholder="0.00" onChange={event => setAmount(event.target.value)} />
              <Input label="Observación (opcional)" value={note} maxLength={100} onChange={event => setNote(event.target.value)} />
              <Button type="submit">{saving ? 'Abriendo caja…' : 'Abrir caja'}</Button>
            </fieldset>
          </form>}
      </>}
  </section>
}
