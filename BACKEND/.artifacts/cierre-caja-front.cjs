const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname, '../../Front-End.web/Front-End');
let file = path.join(root, 'src/features/cash/api/cashApi.ts');
let text = fs.readFileSync(file, 'utf8');
text += `
export type CashClosing = {
  id_sesion_caja: number; id_usuario_apertura: number; id_usuario_cierre: number
  fecha_cierre: string; monto_inicial: number; monto_esperado: number
  monto_contado: number; diferencia: number
}

export function parseCashClosing(value: unknown): CashClosing {
  if (!record(value) || !positiveId(value.id_sesion_caja) || !positiveId(value.id_usuario_apertura) ||
      !positiveId(value.id_usuario_cierre) || typeof value.fecha_cierre !== 'string' ||
      !Number.isFinite(Date.parse(value.fecha_cierre)) ||
      ['monto_inicial', 'monto_esperado', 'monto_contado', 'diferencia'].some(key =>
        typeof value[key] !== 'number' || !Number.isFinite(value[key]) || Math.abs(value[key]) > 99999999.99) ||
      (value.monto_inicial as number) < 0 || (value.monto_contado as number) < 0)
    throw new ApiError('No se pudo interpretar el resultado del cierre. Consulta el estado de la caja.')
  return value as CashClosing
}

export async function closeCash(token: string, idSession: number, amount: number, note: string): Promise<CashClosing> {
  // Se envia el turno revisado, nunca el usuario responsable ni saldos calculados en el navegador.
  if (!positiveId(idSession) || !Number.isFinite(amount) || amount < 0 || amount > 99999999.99 ||
      Math.abs(Math.round(amount * 100) - amount * 100) > 0.00001 || note.length > 100)
    throw new ApiError('Revisa los datos del cierre.')
  const response = parseCashClosing(await requestJson('/Caja/cerrar', { method: 'POST', token, validationMessages: true,
    body: { id_sesion_caja: idSession, monto_contado: amount, observacion_cierre: note.trim() || null } }))
  if (response.id_sesion_caja !== idSession) throw new ApiError('El resultado no corresponde al turno solicitado. Consulta el estado de la caja.')
  return response
}
`;
fs.writeFileSync(file, text);
file = path.join(root, 'src/features/cash/pages/CashPage.tsx');
text = fs.readFileSync(file, 'utf8').replace('loadCash, openCash, parseOpeningAmount', 'loadCash, openCash, closeCash, parseOpeningAmount').replace('AvailableCash, CashSession', 'AvailableCash, CashSession, CashClosing');
text = text.replace('  const [available,', `  const [closing, setClosing] = useState(false)
  const [counted, setCounted] = useState('')
  const [closingNote, setClosingNote] = useState('')
  const [review, setReview] = useState<{ id: number; amount: number; note: string } | null>(null)
  const [result, setResult] = useState<CashClosing | null>(null)
  const money = (value: number) => new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' }).format(value)
  const [available,`);
text = text.replace('      setCurrent(data.current)', '      setClosing(false)\n      setReview(null)\n      setCurrent(data.current)');
text = text.replace("      setNotice('Caja abierta correctamente.')", "      setResult(null)\n      setNotice('Caja abierta correctamente.')");
text = text.replace('  if (!isAdmin) return', `  function reviewClosing(event: FormEvent<HTMLFormElement>) {
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
      setPhase('loading')
      setVersion(value => value + 1)
      busy.current = false
      setSaving(false)
    }
  }

  if (!isAdmin) return`);
text = text.replace('    {error &&', `    {result && <section className="cash-closing-result" aria-labelledby="cash-result-title">
      <h3 id="cash-result-title">Resultado del cierre · turno {result.id_sesion_caja}</h3>
      <dl className="cash-summary">
        <div><dt>Monto esperado</dt><dd>{money(result.monto_esperado)}</dd></div>
        <div><dt>Efectivo contado</dt><dd>{money(result.monto_contado)}</dd></div>
        <div><dt>{result.diferencia === 0 ? 'Diferencia' : result.diferencia > 0 ? 'Sobrante' : 'Faltante'}</dt>
          <dd className={result.diferencia === 0 ? 'cash-success' : 'cash-difference'}>{money(Math.abs(result.diferencia))}</dd></div>
        <div><dt>Fecha de cierre</dt><dd>{new Date(result.fecha_cierre).toLocaleString('es-GT')}</dd></div>
      </dl>
    </section>}
    {error &&`);
text = text.replace('        <p className="cash-hint">Esta caja está disponible para las operaciones del equipo.</p>', `        <p className="cash-hint">Esta caja está disponible para las operaciones del equipo.</p>
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
          </form>}`);
fs.writeFileSync(file, text);
file = path.join(root, 'src/features/cash/pages/CashPage.css');
fs.appendFileSync(file, `
.cash-closing-form, .cash-closing-result { border-top: 1px solid #e2e8f0; padding-top: 20px; }
.cash-closing-result { display: grid; gap: 16px; }
.cash-closing-result h3, .cash-closing-form h3 { margin: 0; font-size: 1rem; }
.cash-actions { display: flex; flex-wrap: wrap; gap: 12px; }
.cash-actions .button { width: auto; flex: 1; }
.cash-panel .cash-close-button { background: #b91c1c; color: white; max-width: 260px; }
.cash-panel .cash-close-button:hover:not(:disabled) { background: #991b1b; }
.cash-panel .cash-close-button:disabled { background: #e2e8f0; color: #475569; }
.cash-panel .cash-secondary { background: white; color: var(--color-text); border-color: var(--color-border); }
.cash-panel .cash-secondary:hover:not(:disabled) { background: #f1f5f9; }
.cash-confirmation { display: grid; gap: 14px; padding: 16px; background: #fff7ed; border-radius: 10px; line-height: 1.6; }
.cash-difference { color: var(--color-warning); }
@media (max-width: 520px) { .cash-actions { flex-direction: column; } .cash-panel .cash-close-button { width: 100%; max-width: none; } .cash-actions .button { max-width: none; } }
`);
