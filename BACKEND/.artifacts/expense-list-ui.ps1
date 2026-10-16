$ErrorActionPreference = 'Stop'
$frontendRoot = 'C:\Users\coy81\Desktop\SISTEMA\SISTEMA\Front-End.web\Front-End'
$pagePath = Join-Path $frontendRoot 'src/features/expenses/pages/ExpensesPage.tsx'
$apiPath = Join-Path $frontendRoot 'src/features/expenses/api/expensesApi.ts'
$schemaPath = Join-Path $frontendRoot 'src/features/expenses/utils/expenseListing.ts'
$cssPath = Join-Path $frontendRoot 'src/features/expenses/pages/ExpensesPage.css'
$page = [System.IO.File]::ReadAllText($pagePath).Replace("`r`n", "`n")
if (-not $page.Contains('<h2 id="expense-title">Registrar gasto</h2>') -or (Test-Path -LiteralPath $schemaPath)) { throw 'Revisar pantalla de gastos antes de editar.' }
$page = "import type { ExpensePage } from '../utils/expenseListing'`n" + $page
$page = $page.Replace('checkExpenseCash, createExpense', 'checkExpenseCash, createExpense, listExpenses')
$page = $page.Replace('  const busy = useRef(false)', @'
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
'@)
$page = $page.Replace("      setDescription(''); setAmount(''); setNote('')", "      setDescription(''); setAmount(''); setNote('')`n      setFormOpen(false); setPage(1)")
$page = $page.Replace('<h2 id="expense-title">Registrar gasto</h2>', '<h2 id="expense-title">Gastos de la caja activa</h2>')
$page = $page.Replace('>Actualizar caja</button>', '>Actualizar</button>')
$page = $page.Replace('    <form onSubmit={submit} aria-busy={saving}>', @'
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
'@)
$page = $page.Replace('  </section>', "    </dialog>`n  </section>")
$api = [System.IO.File]::ReadAllText($apiPath)
$api = "import { parseExpensePage } from '../utils/expenseListing'`n" + $api
$api += @'

export async function listExpenses(token: string, page = 1, signal?: AbortSignal) {
  const response = await requestJson(`/Gastos/listar?pagina=${page}&tamanoPagina=25`, { token, signal })
  try { return parseExpensePage(response) }
  catch { throw new ApiError('El listado de gastos recibido no es válido.') }
}
'@
$css = @'

.expense-panel { max-width: 1100px; margin-bottom: 80px; }
.expense-table-scroll { overflow-x: auto; }
.expense-table { width: 100%; min-width: 680px; border-collapse: collapse; text-align: left; }
.expense-table th, .expense-table td { padding: 12px; border-bottom: 1px solid #e2e8f0; vertical-align: top; font-size: 14px; }
.expense-table th { background: #f8fafc; color: #475569; }
.expense-table td:first-child, .expense-table td:nth-child(3) { overflow-wrap: anywhere; max-width: 260px; }
.expense-table td:nth-child(2) { white-space: nowrap; font-variant-numeric: tabular-nums; }
.expense-table td:last-child { min-width: 160px; }
.expense-pagination { display: flex; justify-content: center; align-items: center; flex-wrap: wrap; gap: 12px; }
.expense-panel button.expense-create-floating { position: fixed; right: max(24px, env(safe-area-inset-right)); bottom: max(24px, env(safe-area-inset-bottom)); z-index: 30; display: grid; place-items: center; width: 60px; height: 60px; padding: 0; border: 0; border-radius: 50%; background: #1d4ed8; color: white; box-shadow: 0 5px 18px #0f172a30; }
.expense-create-dialog { width: min(560px, calc(100% - 24px)); max-height: 90dvh; overflow-y: auto; padding: 24px; border: 0; border-radius: 16px; background: white; color: #0f172a; }
.expense-create-dialog::backdrop { background: #0f172a99; }
.expense-create-dialog form { margin-top: 20px; }
.expense-create-dialog .expense-error { margin-top: 16px; }
@media (max-width: 520px) { .expense-create-dialog { padding: 18px; } }
'@
$utf8 = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($schemaPath, [System.IO.File]::ReadAllText((Join-Path $PSScriptRoot 'expenseListing.ts')), $utf8)
[System.IO.File]::WriteAllText($pagePath, $page, $utf8)
[System.IO.File]::WriteAllText($apiPath, $api, $utf8)
[System.IO.File]::AppendAllText($cssPath, $css, $utf8)
