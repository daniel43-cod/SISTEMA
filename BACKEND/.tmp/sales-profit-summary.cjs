const fs=require('node:fs'),path=require('node:path');
const root=path.resolve(__dirname,'../../Front-End.web/Front-End');
function edit(file,a,b){const p=path.join(root,file),s=fs.readFileSync(p,'utf8');if(!s.includes(a))throw Error(a);fs.writeFileSync(p,s.replace(a,b));}
fs.writeFileSync(path.join(root,'src/features/sales/api/saleSummaryApi.ts'), `import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'
import type { SaleHistory } from './salesApi'

export async function loadSaleSummary(token: string, page: number, pageSize: number, range?: { desde: string; hasta: string }, userId?: number, signal?: AbortSignal) {
  const params = new URLSearchParams({ pagina: String(page), tamanoPagina: String(pageSize) })
  if (range) { params.set('desde', range.desde); params.set('hasta', range.hasta) }
  if (userId !== undefined) params.set('idUsuario', String(userId))
  const v = await requestJson('/Venta/resumen?' + params, { token, signal }) as Record<string, unknown> | null
  const finite = (value: unknown) => typeof value === 'number' && Number.isFinite(value)
  if (!v || v.pagina !== page || v.tamanoPagina !== pageSize || !Number.isSafeInteger(v.totalRegistros) || Number(v.totalRegistros) < 0 ||
    typeof v.hayMas !== 'boolean' || !finite(v.gananciaTotal) || !Array.isArray(v.items) || v.items.length > pageSize ||
    !v.items.every(item => item && Number.isSafeInteger(item.id_ventas) && item.id_ventas > 0 && typeof item.cliente === 'string' &&
      typeof item.usuario === 'string' && finite(item.total) && finite(item.subtotal) && (item.descuento === null || finite(item.descuento)) &&
      finite(item.saldo_pendiente) && finite(item.ganancia_total) && typeof item.estado === 'string' && typeof item.fecha_venta === 'string'))
    throw new ApiError('El resumen de ventas no tiene el formato esperado.')
  return { items: v.items as SaleHistory[], hayMas: v.hayMas, gananciaTotal: v.gananciaTotal as number }
}
`);
const page='src/features/sales/pages/SaleListPage.tsx';
edit(page,"import { loadSaleHistory }", "import { loadSaleSummary } from '../api/saleSummaryApi'\nimport { loadSaleHistory }");
edit(page,"  const [hasNext, setHasNext]", "  const [profitTotal, setProfitTotal] = useState<number | null>(null)\n  const [profitVisible, setProfitVisible] = useState(false)\n  const [hasNext, setHasNext]");
edit(page,"setLoading(true); setError(''); setRows([]); setHasNext(false)", "setLoading(true); setError(''); setRows([]); setHasNext(false); setProfitTotal(null)");
edit(page,'      try {\n        const result = await loadSaleHistory',`      try {
        if (isAdmin) {
          const summary = await loadSaleSummary(token!, page, pageSize, range, userId ? Number(userId) : undefined, controller.signal)
          if (controller.signal.aborted) return
          if (summary.items.length === 0 && page > 1) { setPage(1); return }
          setRows(summary.items); setHasNext(summary.hayMas); setProfitTotal(summary.gananciaTotal)
          return
        }
        const result = await loadSaleHistory`);
edit(page,'      <div className="sale-list-body" aria-busy={loading}>', `      {isAdmin && <div className="sale-profit-card">
        <span>Ganancia total</span>
        <div><strong aria-label={profitVisible ? undefined : 'Ganancia oculta'}>
          {profitVisible ? (profitTotal === null ? '—' : money.format(profitTotal)) : 'Q ••••••'}
        </strong>
        <button type="button" className="sale-profit-toggle" aria-label={profitVisible ? 'Ocultar ganancia total' : 'Mostrar ganancia total'}
          title={profitVisible ? 'Ocultar ganancia' : 'Mostrar ganancia'} aria-pressed={profitVisible} onClick={() => setProfitVisible(v => !v)}>
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12Z" /><circle cx="12" cy="12" r="3" />
            {!profitVisible && <path d="m3 3 18 18" />}
          </svg>
        </button></div>
      </div>}
      <div className="sale-list-body" aria-busy={loading}>`);
fs.appendFileSync(path.join(root,'src/features/sales/pages/SaleListPage.css'), `
.sale-profit-card { flex-shrink: 0; align-self: flex-end; min-width: 180px; padding: 8px 12px; margin-bottom: 12px; border: 1px solid var(--border); border-radius: 10px; background: #eff8ff; }
.sale-profit-card > span { font-size: .8rem; color: var(--text-muted); }
.sale-profit-card > div { display: flex; align-items: center; justify-content: space-between; gap: 12px; }
.sale-profit-card strong { font-size: 1rem; font-variant-numeric: tabular-nums; }
.sale-list-view button.sale-profit-toggle { display: inline-flex; align-items: center; justify-content: center; width: 44px; min-height: 44px; padding: 6px; border: 0; background: transparent; }
`);
