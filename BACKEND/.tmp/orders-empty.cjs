const fs=require('fs');const root='C:/Users/coy81/Desktop/SISTEMA/SISTEMA/Front-End.web/Front-End/src/features/orders/pages/';const p=root+'OrdersPage.tsx';let s=fs.readFileSync(p,'utf8');
const pos=s.indexOf('export function OrdersPage()');let before=s.slice(0,pos),body=s.slice(pos);
body=body.replace("  const [success, setSuccess] = useState('')",`  const [success, setSuccess] = useState('')
  const [updatedAt, setUpdatedAt] = useState<Date | null>(null)
  const [copyStatus, setCopyStatus] = useState('')
  async function copyCatalogLink() {
    try {
      await navigator.clipboard.writeText(new URL(import.meta.env.BASE_URL + 'catalogo', window.location.origin).href)
      setCopyStatus('Enlace copiado.')
    } catch {
      setCopyStatus('No se pudo copiar el enlace. Abre el catálogo y copia la dirección del navegador.')
    }
  }`);
body=body.replace('      else setData(v)','      else { setData(v); setUpdatedAt(new Date()); setCopyStatus(\'\') }');
body=body.replace('<p role="status">No hay pedidos pendientes de aprobación.</p>',`<div className="orders-empty-state">
          <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d="m4 4-3 10v6h22v-6L20 4Z" /><path d="M1 14h6l2 3h6l2-3h6" />
          </svg>
          <h3>Todo al día</h3>
          <p>No hay pedidos esperando aprobación. Los nuevos pedidos del catálogo aparecerán aquí.</p>
          {updatedAt && <small>Última actualización: <time dateTime={updatedAt.toISOString()}>{updatedAt.toLocaleTimeString('es-GT', { hour: 'numeric', minute: '2-digit' })}</time></small>}
          <button type="button" className="orders-copy-link" onClick={() => void copyCatalogLink()}>Copiar enlace del catálogo</button>
          {copyStatus && <p role="status">{copyStatus}</p>}
        </div>`);
body=body.replace('<nav className="sales-history-pagination" aria-label="Páginas de pedidos">', '{!loading && !error && !!data?.pedidos.length && (page > 1 || data.hayMas) && <nav className="sales-history-pagination" aria-label="Páginas de pedidos">');
body=body.replace('Siguiente</button></nav>', 'Siguiente</button></nav>}');
fs.writeFileSync(p,before+body);
fs.appendFileSync(root+'OrdersPage.css',`
.orders-empty-state { display: flex; flex-direction: column; align-items: center; gap: 12px; padding: 48px 20px; text-align: center; }
.orders-empty-state > svg { color: var(--text-muted, #64748b); }
.orders-empty-state h3 { margin: 0; font-size: 1.15rem; }
.orders-empty-state p { margin: 0; max-width: 440px; line-height: 1.6; }
.orders-empty-state small { color: var(--text-muted, #64748b); }
.sales-page .orders-empty-state .orders-copy-link { border: 0; background: transparent; color: var(--color-primary, #075b96); text-decoration: underline; padding: 8px; font-size: .9rem; }
`);
