import type { ExpensePage } from '../utils/expenseListing'
import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { checkExpenseCash, createExpense, listExpenses } from '../api/expensesApi'
import { buildExpense } from '../utils/expenseValidation'
import './ExpensesPage.css'

export function ExpensesPage() {
  const { session, logout } = useAuth()
  const token = session?.token
  const [description, setDescription] = useState('')
  const [amount, setAmount] = useState('')
  const [note, setNote] = useState('')
  const [loading, setLoading] = useState(true)
  const [cashOpen, setCashOpen] = useState(false)
  const [cashError, setCashError] = useState('')
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [saving, setSaving] = useState(false)
  const [uncertain, setUncertain] = useState(false)
  const [revision, setRevision] = useState(0)
  const [formOpen, setFormOpen] = useState(false)
  const [page, setPage] = useState(1)
  const [history, setHistory] = useState<ExpensePage | null>(null)
  const [historyLoading, setHistoryLoading] = useState(true)
  const [historyError, setHistoryError] = useState('')
  const dialogRef = useRef<HTMLDialogElement>(null)
  useEffect(() => {
    if (!formOpen || !dialogRef.current) return
    const dialog = dialogRef.current
    dialog.showModal()
    const previous = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    return () => { dialog.close(); document.body.style.overflow = previous }
  }, [formOpen])
  useEffect(() => {
    if (!token) return
    const controller = new AbortController()
    async function loadHistory() {
      setHistoryLoading(true); setHistoryError('')
      try {
        const result = await listExpenses(token!, page, controller.signal)
        if (!controller.signal.aborted) {
          const lastPage = Math.max(1, Math.ceil(result.total / result.tamanoPagina))
          if (page > lastPage) setPage(lastPage)
          else setHistory(result)
        }
      } catch (failure) {
        if (controller.signal.aborted) return
        if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
        else setHistoryError(failure instanceof Error ? failure.message : 'No se pudieron cargar los gastos.')
      } finally { if (!controller.signal.aborted) setHistoryLoading(false) }
    }
    void loadHistory()
    return () => controller.abort()
  }, [token, page, revision, logout])
  const busy = useRef(false)
  const alive = useRef(true)
  useEffect(() => { alive.current = true; return () => { alive.current = false } }, [])
  useEffect(() => {
    if (!token) return
    const controller = new AbortController()
    async function load() {
      setLoading(true); setCashError('')
      try {
        const cash = await checkExpenseCash(token!, controller.signal)
        if (!controller.signal.aborted) setCashOpen(cash !== null)
      } catch (failure) {
        if (controller.signal.aborted) return
        if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
        else { setCashOpen(false); setCashError(failure instanceof Error ? failure.message : 'No se pudo consultar la caja.') }
      } finally { if (!controller.signal.aborted) setLoading(false) }
    }
    void load()
    return () => controller.abort()
  }, [token, revision, logout])
  if (session?.user.role !== 'ADMINISTRADOR') return null

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!token || busy.current || uncertain || loading || !cashOpen || cashError) return
    setError(''); setSuccess('')
    let payload
    try { payload = buildExpense(description, amount, note) }
    catch (failure) { setError(failure instanceof Error ? failure.message : 'Revisa los datos.'); return }
    busy.current = true; setSaving(true)
    try {
      const id = await createExpense(token, payload)
      if (!alive.current) return
      setSuccess(`Gasto #${id} registrado por ${new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' }).format(payload.monto)}.`)
      setDescription(''); setAmount(''); setNote('')
      setFormOpen(false); setPage(1)
      setRevision(v => v + 1)
    } catch (failure) {
      if (!alive.current) return
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else {
        const ambiguous = !(failure instanceof ApiError) || failure.status === 0 || failure.status >= 500
        setUncertain(ambiguous)
        setError(ambiguous ? 'No se pudo confirmar el registro. Conservamos los datos. Revisa los movimientos de caja antes de volver a enviar para evitar duplicados.'
          : failure.message)
        setRevision(v => v + 1)
      }
    } finally { busy.current = false; if (alive.current) setSaving(false) }
  }
  return <section className="expense-panel" aria-labelledby="expense-title">
    <div className="expense-heading"><h2 id="expense-title">Gastos de la caja activa</h2>
      <button type="button" disabled={loading || saving} onClick={() => setRevision(v => v + 1)}>Actualizar</button></div>
    <p className={cashOpen && !cashError ? 'expense-open' : 'expense-hint'} role="status">
      {loading ? 'Consultando caja…' : cashError ? 'No se pudo verificar la caja.'
        : cashOpen ? 'Caja abierta. El gasto se registrará como una salida del turno actual.'
        : 'Abre la caja para registrar un gasto.'}
    </p>
    {cashError && <p role="alert" className="expense-error">{cashError}</p>}
    {success && <p role="status" className="expense-success">{success}</p>}
    {historyLoading ? <p role="status">Cargando gastos…</p>
      : historyError ? <p className="expense-error" role="alert">{historyError}</p>
      : !history?.items.length ? <p>No hay gastos registrados en esta página de la caja activa.</p>
      : <div className="expense-table-scroll"><table className="expense-table">
        <thead><tr><th>Descripción</th><th>Monto</th><th>Observación</th><th>Encargado</th><th>Fecha y hora</th></tr></thead>
        <tbody>{history.items.map(item => <tr key={item.idGasto}>
          <td>{item.descripcion}</td>
          <td>{new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' }).format(item.monto)}</td>
          <td>{item.observacion || '—'}</td><td>{item.encargado}</td>
          <td><time dateTime={item.fecha}>{new Date(item.fecha).toLocaleString('es-GT')}</time></td>
        </tr>)}</tbody>
      </table></div>}
    <nav className="expense-pagination" aria-label="Páginas de gastos">
      <button type="button" disabled={historyLoading || page === 1} onClick={() => setPage(v => v - 1)}>Anterior</button>
      <span>Página {page} de {Math.max(1, Math.ceil((history?.total ?? 0) / 25))}</span>
      <button type="button" disabled={historyLoading || !!historyError || !history || page * 25 >= history.total}
        onClick={() => setPage(v => v + 1)}>Siguiente</button>
    </nav>
    <button type="button" className="expense-create-floating" title="Registrar nuevo gasto"
      aria-label="Registrar nuevo gasto" aria-haspopup="dialog" aria-controls="expense-create-dialog"
      disabled={loading || saving || !cashOpen || !!cashError} onClick={() => setFormOpen(true)}>
      <svg width="26" height="26" viewBox="0 0 24 24" fill="none" stroke="currentColor"
        strokeWidth="2" strokeLinecap="round" aria-hidden="true"><path d="M12 5v14M5 12h14" /></svg>
    </button>
    <dialog id="expense-create-dialog" ref={dialogRef} className="expense-create-dialog"
      aria-labelledby="expense-create-title" onCancel={event => { if (saving) event.preventDefault(); else setFormOpen(false) }}
      onClose={() => setFormOpen(false)}>
      <div className="expense-heading"><h2 id="expense-create-title">Registrar gasto</h2>
        <button type="button" disabled={saving} aria-label="Cerrar formulario de gasto" onClick={() => setFormOpen(false)}>×</button>
      </div>
    <form onSubmit={submit} aria-busy={saving}>
      <fieldset disabled={loading || saving || !cashOpen || !!cashError || uncertain}>
        <legend className="expense-sr-only">Datos del gasto</legend>
        <label htmlFor="expense-description">Descripción</label>
        <input id="expense-description" value={description} maxLength={300} required
          placeholder="Ejemplo: transporte de mercadería" onChange={event => setDescription(event.target.value)} />
        <label htmlFor="expense-amount">Monto (Q)</label>
        <input id="expense-amount" value={amount} inputMode="decimal" maxLength={11} required
          placeholder="0.00" onChange={event => setAmount(event.target.value)} />
        <label htmlFor="expense-note">Observación (opcional)</label>
        <textarea id="expense-note" value={note} maxLength={500} rows={3}
          onChange={event => setNote(event.target.value)} />
        <button className="expense-submit" type="submit">{saving ? 'Registrando…' : 'Registrar gasto'}</button>
      </fieldset>
    </form>
    {error && <p role="alert" className="expense-error">{error}</p>}
    {uncertain && <button type="button" onClick={() => { setUncertain(false); setError('') }}>
      Ya revisé los movimientos y el gasto no se registró
    </button>}
    </dialog>
  </section>
}
