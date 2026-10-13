const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname, '../../Front-End.web/Front-End');
function write(relative, content) {
  const target = path.join(root, relative);
  fs.mkdirSync(path.dirname(target), { recursive: true });
  fs.writeFileSync(target, content, 'utf8');
}
write('src/features/cash/api/cashApi.ts', `import { requestJson } from '../../../shared/api/httpClient'
import { ApiError } from '../../../shared/api/ApiError'

export type AvailableCash = { idCaja: number; nombre: string }
export type CashSession = {
  id_sesion_caja: number; id_caja: number; usuario_apertura: string
  fecha_apertura: string; monto_inicial: number
}
const record = (value: unknown): value is Record<string, unknown> => typeof value === 'object' && value !== null
const positiveId = (value: unknown): value is number => typeof value === 'number' && Number.isSafeInteger(value) && value > 0

// Verifica el contrato antes de mostrar datos o habilitar una nueva apertura.
export function parseCashSession(value: unknown): CashSession | null {
  if (value === null) return null
  if (!record(value) || !positiveId(value.id_sesion_caja) || !positiveId(value.id_caja) ||
      typeof value.usuario_apertura !== 'string' || typeof value.fecha_apertura !== 'string' ||
      !Number.isFinite(Date.parse(value.fecha_apertura)) || typeof value.monto_inicial !== 'number' ||
      !Number.isFinite(value.monto_inicial) || value.monto_inicial < 0 || value.fecha_cierre !== null)
    throw new ApiError('No se pudo interpretar el estado de la caja.')
  return value as CashSession
}

export function parseAvailableCash(value: unknown): AvailableCash[] {
  if (!Array.isArray(value) || value.length > 100 || value.some(item => !record(item) ||
      !positiveId(item.idCaja) || typeof item.nombre !== 'string'))
    throw new ApiError('No se pudieron interpretar las cajas disponibles.')
  return value as AvailableCash[]
}

export async function loadCash(token: string, signal?: AbortSignal) {
  const [current, available] = await Promise.all([
    requestJson('/Caja/actual', { token, signal }),
    requestJson('/Caja/disponibles', { token, signal }),
  ])
  return { current: parseCashSession(current), available: parseAvailableCash(available) }
}

export function parseOpeningAmount(value: string): number | null {
  const normalized = value.trim().replace(',', '.')
  if (!/^\u005cd{1,8}(?:[.]\u005cd{1,2})?$/.test(normalized)) return null
  const amount = Number(normalized)
  return Number.isFinite(amount) && amount <= 99999999.99 ? amount : null
}

export async function openCash(token: string, idCaja: number, amount: number, note: string) {
  await requestJson('/Caja/abrir', { method: 'POST', token, validationMessages: true,
    body: { id_caja: idCaja, monto_inicial: amount, observacion: note.trim() || null } })
}
`);
write('src/features/cash/pages/CashPage.tsx', `import { useEffect, useRef, useState } from 'react'
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
    setPhase('loading')
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
      <RefreshButton label="Actualizar estado de caja" loading={saving || phase === 'loading'} onClick={() => { setError(''); setVersion(value => value + 1) }} />
    </div>
    {notice && <p className="cash-success" role="status">{notice}</p>}
    {error && <p className="cash-error" role="alert">{error}</p>}
    {phase === 'loading' ? <p role="status">Consultando caja…</p> : phase === 'error' ?
      <Button onClick={() => { setError(''); setVersion(value => value + 1) }}>Volver a consultar</Button> :
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
`);
write('src/features/cash/pages/CashPage.css', `.cash-panel { width: 100%; max-width: 760px; padding: clamp(16px, 4vw, 28px); background: var(--color-surface); color: var(--color-text); border: 1px solid #e2e8f0; border-radius: 16px; display: grid; gap: 20px; }
.cash-heading { display: flex; justify-content: space-between; align-items: center; gap: 12px; }
.cash-heading h2 { font-size: 1.2rem; }
.cash-form, .cash-form fieldset { display: grid; gap: 20px; min-width: 0; }
.cash-form fieldset { border: 0; padding: 0; margin: 0; }
.cash-form select { font: inherit; }
.cash-form .button { max-width: 260px; }
.cash-status { font-weight: 650; }
.cash-status.is-open, .cash-success { color: var(--color-success); }
.cash-status.is-closed { color: var(--color-warning); }
.cash-error { color: var(--color-error); }
.cash-hint { color: var(--color-muted); line-height: 1.6; }
.cash-summary { margin: 0; display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
.cash-summary dt { color: var(--color-muted); font-size: .875rem; margin-bottom: 6px; }
.cash-summary dd { margin: 0; font-weight: 600; overflow-wrap: anywhere; }
@media (max-width: 520px) { .cash-summary { grid-template-columns: 1fr; } .cash-form .button { max-width: none; } }
`);
let target = path.join(root, 'src/app/layouts/StaffLayout.tsx');
let content = fs.readFileSync(target, 'utf8');
if (!content.includes("import { CashPage }")) {
  content = "import { CashPage } from '../../features/cash/pages/CashPage'\n" + content;
  content = content.replace("current.id === 'productos' ? <ProductsPage /> :", "current.id === 'productos' ? <ProductsPage /> : current.id === 'caja' ? <CashPage /> :");
  fs.writeFileSync(target, content);
}
target = path.join(root, 'src/app/navigation/navigationItems.ts');
content = fs.readFileSync(target, 'utf8').replace("description: 'Apertura, movimientos y cierre de caja.', roles: staff", "description: 'Consulta y apertura de la caja compartida.', roles: admin");
fs.writeFileSync(target, content);
