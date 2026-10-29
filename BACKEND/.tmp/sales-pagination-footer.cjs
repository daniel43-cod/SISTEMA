const fs=require('node:fs'),path=require('node:path');
const root=path.resolve(__dirname,'../../Front-End.web/Front-End');
function edit(file,a,b){const p=path.join(root,file),s=fs.readFileSync(p,'utf8');if(!s.includes(a))throw Error(a);fs.writeFileSync(p,s.replace(a,b));}
edit('src/features/sales/utils/saleHistoryQuery.ts','userId?: number) {','userId?: number, pageSize = 50) {');
edit('src/features/sales/utils/saleHistoryQuery.ts',"tamanoPagina: '50'",'tamanoPagina: String(pageSize)');
edit('src/features/sales/api/salesApi.ts','userId?: number): Promise<SaleHistory[]>','userId?: number, pageSize = 50): Promise<SaleHistory[]>');
edit('src/features/sales/api/salesApi.ts','saleHistoryQuery(page, range, userId)','saleHistoryQuery(page, range, userId, pageSize)');
const file='src/features/sales/pages/SaleListPage.tsx',p=path.join(root,file);let s=fs.readFileSync(p,'utf8');
s=s.replace('  const [page, setPage] = useState(1)', '  const [page, setPage] = useState(1)\n  const [pageSize, setPageSize] = useState(50)');
s=s.replaceAll('isAdmin && userId ? Number(userId) : undefined)', 'isAdmin && userId ? Number(userId) : undefined, pageSize)');
s=s.replace('result.length === 50','result.length === pageSize');
s=s.replace('exactamente 50 ventas.', 'exactamente el tamaño de página.');
s=s.replace('range, isAdmin, userId]', 'range, isAdmin, userId, pageSize]');
s=s.replace('className="sales-page sale-list-page"', 'className="sales-page sale-list-page sale-list-view"');
s=s.replace('<div aria-busy={loading}>', '<div className="sale-list-body" aria-busy={loading}>');
const start=s.indexOf('      <nav className="sales-history-pagination"');const end=s.indexOf('      </nav>',start);
if(start<0||end<0)throw Error('pagination');
s=s.slice(0,start)+`      <footer className="sale-list-footer">
        <span role="status">{loading ? 'Cargando…' : error ? 'No se pudo cargar el listado' : rows.length === 0 ? 'Sin ventas' :
          'Mostrando ' + ((page - 1) * pageSize + 1) + '–' + ((page - 1) * pageSize + rows.length) + (page === 1 && !hasNext ? ' de ' + rows.length : '')}</span>
        <label className="sale-list-page-size">Filas por página
          <select value={pageSize} disabled={loading || saving} onChange={event => { setPageSize(Number(event.target.value)); setPage(1) }}>
            <option value={10}>10</option><option value={25}>25</option><option value={50}>50</option>
          </select>
        </label>
        {!error && (page > 1 || hasNext) && <nav className="sales-history-pagination" aria-label="Páginas de ventas">
          <button type="button" disabled={loading || saving || page === 1} onClick={() => setPage(v => v - 1)}>Anterior</button>
          <span>Página {page}</span>
          <button type="button" disabled={loading || saving || !hasNext} onClick={() => setPage(v => v + 1)}>Siguiente</button>
        </nav>}
      </footer>`+s.slice(end+'      </nav>'.length);
fs.writeFileSync(p,s);
edit('src/app/layouts/StaffLayout.tsx','className={`staff-shell ${', "className={`staff-shell ${current.id === 'listado-ventas' ? 'staff-shell-sale-list ' : ''}${");
fs.appendFileSync(path.join(root,'src/app/layouts/StaffLayout.css'), `
.staff-shell-sale-list .staff-workspace { display: flex; flex-direction: column; min-height: 100dvh; }
.staff-shell-sale-list .staff-header { flex-shrink: 0; }
.staff-shell-sale-list .staff-content { flex: 1; display: flex; width: 100%; }
`);
fs.appendFileSync(path.join(root,'src/features/sales/pages/SaleListPage.css'), `
.sale-list-view { flex: 1; display: flex; flex-direction: column; min-width: 0; }
.sale-list-view > .sales-card { flex: 1; display: flex; flex-direction: column; }
.sale-list-body { flex: 1; }
.sale-list-footer { display: flex; align-items: center; justify-content: space-between; gap: 16px; flex-wrap: wrap; margin-top: auto; padding-top: 20px; border-top: 1px solid var(--border); font-size: .875rem; }
.sale-list-page .sale-list-footer .sales-history-pagination { margin-top: 0; }
.sale-list-page .sale-list-page-size { display: flex; align-items: center; gap: 8px; }
.sale-list-page-size select { width: auto; }
`);
fs.appendFileSync(path.join(root,'tests/sales.test.ts'), `

test('El tamaño seleccionado se envía en todos los modos del listado de ventas', () => {
  for (const size of [10, 25, 50]) {
    for (const url of [saleHistoryQuery(1, undefined, undefined, size), saleHistoryQuery(2, undefined, 2, size),
      saleHistoryQuery(1, { desde: '2026-10-23', hasta: '2026-10-23' }, 2, size)])
      assert.equal(new URL(url, 'https://example.test').searchParams.get('tamanoPagina'), String(size))
  }
})
`);
