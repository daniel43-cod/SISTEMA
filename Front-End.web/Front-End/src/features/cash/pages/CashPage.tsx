import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { useAuth } from '../../auth'
import { Input } from '../../../shared/ui/Input'
import { Button } from '../../../shared/ui/Button'
import { RefreshButton } from '../../../shared/ui/RefreshButton'
import { ApiError } from '../../../shared/api/ApiError'
import { loadCash, openCash, parseOpeningAmount } from '../api/cashApi'
import type { AvailableCash, CashSession } from '../api/cashApi'
import './CashPage.css'

export function CashPage() {
  const { session } = useAuth()
  const [current, setCurrent] = useState<CashSession | null>(null)
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
      setNotice('Caja abierta correctamente.')
      setAmount('')
      setNote('')
    } catch (cause) {
      setError(cause instanceof ApiError && cause.status === 409
        ? 'El estado de la caja cambió. Revisa la información actualizada.'
        : cause instanceof ApiError ? cause.message : 'No se pudo abrir la caja.')
    } finally {
      // Siempre consulta el estado real: un timeout no prueba que la apertura haya fallado.
      setPhase('loading')
      setVersion(value => value + 1)
      busy.current = false
      setSaving(false)
    }
  }

  if (!isAdmin) return <p>No tienes permiso para administrar la caja.</p>
  return <section className="cash-panel" aria-labelledby="cash-title" aria-busy={phase === 'loading' || saving}>
    <div className="cash-heading">
      <h2 id="cash-title">Sesión de caja</h2>
      <RefreshButton label="Actualizar estado de caja" loading={saving || phase === 'loading'} onClick={() => { setError(''); setPhase('loading'); setVersion(value => value + 1) }} />
    </div>
    {notice && <p className="cash-success" role="status">{notice}</p>}
    {error && <p className="cash-error" role="alert">{error}</p>}
    {phase === 'loading' ? <p role="status">Consultando caja…</p> : phase === 'error' ?
      <Button onClick={() => { setError(''); setPhase('loading'); setVersion(value => value + 1) }}>Volver a consultar</Button> :
      current ? <>
        <p className="cash-status is-open"><span aria-hidden="true">●</span> Caja abierta</p>
        <dl className="cash-summary">
          <div><dt>Caja</dt><dd>{available.find(item => item.idCaja === current.id_caja)?.nombre ?? 'Caja del turno actual'}</dd></div>
          <div><dt>Monto inicial</dt><dd>{new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' }).format(current.monto_inicial)}</dd></div>
          <div><dt>Abierta por</dt><dd>{current.usuario_apertura}</dd></div>
          <div><dt>Fecha de apertura</dt><dd>{new Date(current.fecha_apertura).toLocaleString('es-GT')}</dd></div>
        </dl>
        <p className="cash-hint">Esta caja está disponible para las operaciones del equipo.</p>
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
