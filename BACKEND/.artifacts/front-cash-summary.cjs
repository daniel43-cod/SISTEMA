const fs = require('fs');
const root = 'C:/Users/coy81/Desktop/SISTEMA/SISTEMA/Front-End.web/Front-End/';
const api = 'src/features/cash/api/cashApi.ts';
fs.appendFileSync(root + api, `
export type CashMovement = {
  id_movimiento_caja: number; id_sesion_caja: number; tipo_movimiento: string
  naturaleza: string; usuario: string; fecha_movimiento: string; monto: number; descripcion: string | null
}
export type CashSummary = {
  id_sesion_caja: number; monto_inicial: number; total_entradas: number; total_salidas: number
  saldo_esperado: number; total_movimientos: number; movimientos: CashMovement[]
}

// Los totales proceden del servidor: los 100 movimientos visibles no representan toda la sesion.
export function parseCashSummary(value: unknown, idSession: number): CashSummary {
  const finite = (item: unknown): item is number => typeof item === 'number' && Number.isFinite(item)
  if (!record(value) || !positiveId(value.id_sesion_caja) || value.id_sesion_caja !== idSession ||
      !finite(value.monto_inicial) || value.monto_inicial < 0 ||
      !finite(value.total_entradas) || value.total_entradas < 0 ||
      !finite(value.total_salidas) || value.total_salidas < 0 || !finite(value.saldo_esperado) ||
      typeof value.total_movimientos !== 'number' || !Number.isSafeInteger(value.total_movimientos) || value.total_movimientos < 0 ||
      !Array.isArray(value.movimientos) || value.movimientos.length !== Math.min(100, value.total_movimientos) ||
      value.movimientos.some(m => !record(m) || !positiveId(m.id_movimiento_caja) || m.id_sesion_caja !== idSession ||
        typeof m.tipo_movimiento !== 'string' || typeof m.naturaleza !== 'string' ||
        !['ENTRADA', 'INGRESO', 'SALIDA', 'EGRESO'].includes(m.naturaleza.trim().toUpperCase()) ||
        typeof m.usuario !== 'string' || typeof m.fecha_movimiento !== 'string' || !Number.isFinite(Date.parse(m.fecha_movimiento)) ||
        !finite(m.monto) || m.monto <= 0 || (m.descripcion !== null && typeof m.descripcion !== 'string')))
    throw new ApiError('No se pudo interpretar el resumen de caja.')
  if (new Set(value.movimientos.map(m => m.id_movimiento_caja)).size !== value.movimientos.length)
    throw new ApiError('El resumen de caja contiene movimientos duplicados.')
  return value as CashSummary
}

export async function loadCashSummary(token: string, idSession: number, signal?: AbortSignal): Promise<CashSummary> {
  if (!positiveId(idSession)) throw new ApiError('El turno de caja no es válido.')
  return parseCashSummary(await requestJson('/MovimientoCaja/listar?idSesionCaja=' + idSession, { token, signal }), idSession)
}
`);
const page = root + 'src/features/cash/pages/CashPage.tsx';
let s = fs.readFileSync(page, 'utf8');
s = s.replace('loadCash, openCash,', 'loadCash, loadCashSummary, openCash,').replace('AvailableCash, CashSession, CashClosing', 'AvailableCash, CashSession, CashClosing, CashSummary');
s = s.replace("  const [closing, setClosing]", "  const [summary, setSummary] = useState<CashSummary | null>(null)\n  const [summaryError, setSummaryError] = useState('')\n  const [closing, setClosing]");
const marker = '  async function submit(';
const idx = s.indexOf(marker);
s = s.slice(0, idx) + `  const currentId = current?.id_sesion_caja
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

` + s.slice(idx);
// Clear the previous snapshot on each state refresh, including opening/closing and retry.
s = s.replaceAll("setPhase('loading')", "setSummary(null); setSummaryError(''); setPhase('loading')");
const target = '        <p className="cash-hint">Esta caja está disponible para las operaciones del equipo.</p>';
s = s.replace(target, target + `
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
        </section>`);
fs.writeFileSync(page, s);
fs.appendFileSync(root + 'src/features/cash/pages/CashPage.css', `
.cash-movements { display: grid; gap: 16px; min-width: 0; border-top: 1px solid var(--color-border, #e2e8f0); padding-top: 20px; }
.cash-movements h3 { margin: 0; font-size: 1rem; }
.cash-incoming { color: #15803d; }
.cash-outgoing { color: #b45309; }
.cash-movement-list { list-style: none; padding: 0; margin: 0; max-height: 480px; overflow-y: auto; }
.cash-movement-list li { display: flex; align-items: center; justify-content: space-between; gap: 16px; padding: 14px 4px; border-bottom: 1px solid var(--color-border, #e2e8f0); }
.cash-movement-list li > div { display: grid; gap: 5px; }
.cash-movement-list li > div:last-child { text-align: right; flex-shrink: 0; }
.cash-movement-info { min-width: 0; overflow-wrap: anywhere; }
.cash-movement-info span { font-size: .85rem; color: var(--color-muted); }
@media (max-width: 520px) { .cash-movement-list li { align-items: flex-start; gap: 10px; } .cash-movement-list li > div:last-child { font-size: .85rem; } }
`);
fs.appendFileSync(root + 'tests/cash.test.ts', `
test('Resumen usa totales del backend aunque solo entregue los ultimos 100', async t => {
  const { loadCashSummary, parseCashSummary } = await import('../src/features/cash/api/cashApi.ts')
  const data = { id_sesion_caja: 7, monto_inicial: 50, total_entradas: 1000, total_salidas: 500,
    saldo_esperado: 550, total_movimientos: 2000, movimientos: Array.from({ length: 100 }, (_, i) => ({
      id_movimiento_caja: i + 1, id_sesion_caja: 7, tipo_movimiento: 'Compra', naturaleza: ' Salida ',
      usuario: 'Admin', fecha_movimiento: '2026-10-07T18:00:00', monto: 1, descripcion: null })) }
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/MovimientoCaja/listar?idSesionCaja=7')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    return Response.json(data)
  })
  assert.deepEqual(await loadCashSummary('token', 7), data)
  assert.throws(() => parseCashSummary(data, 8), ApiError)
  assert.throws(() => parseCashSummary({ ...data, total_salidas: NaN }, 7), ApiError)
  assert.throws(() => parseCashSummary({ ...data, movimientos: [{ ...data.movimientos[0], id_sesion_caja: 8 }] }, 7), ApiError)
  assert.throws(() => parseCashSummary({ ...data, movimientos: Array(100).fill(data.movimientos[0]) }, 7), ApiError)
  assert.deepEqual(parseCashSummary({ ...data, total_movimientos: 0, movimientos: [] }, 7).movimientos, [])
})
`);
