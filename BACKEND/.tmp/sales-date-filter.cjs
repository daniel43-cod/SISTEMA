const fs = require('node:fs'), path = require('node:path');
const root = path.resolve(__dirname, '../../Front-End.web/Front-End');
function edit(file, a, b) { const p = path.join(root,file), s = fs.readFileSync(p,'utf8'); if (!s.includes(a)) throw Error(a); fs.writeFileSync(p,s.replace(a,b)); }
fs.writeFileSync(path.join(root,'src/features/sales/utils/saleDateRange.ts'), `export function lastSevenSaleDays(now = new Date()) {
  const parts = new Intl.DateTimeFormat('en-US', { timeZone: 'America/Guatemala', year: 'numeric', month: '2-digit', day: '2-digit' }).formatToParts(now)
  const part = (type: string) => parts.find(p => p.type === type)!.value
  const hasta = part('year') + '-' + part('month') + '-' + part('day')
  const start = new Date(hasta + 'T00:00:00Z')
  start.setUTCDate(start.getUTCDate() - 6)
  return { desde: start.toISOString().slice(0,10), hasta }
}
`);
const api='src/features/sales/api/salesApi.ts';
edit(api, 'page = 1, userId?: number', 'page = 1, range?: { desde: string; hasta: string }');
edit(api, "const route = userId === undefined ? '/Venta/listar' : '/Venta/caja-activa/usuario/' + userId\n  const result = await requestJson(route + '?pagina=' + page + '&tamanoPagina=50', { token, signal })", "const params = new URLSearchParams({ pagina: String(page), tamanoPagina: '50' })\n  if (range) { params.set('desde', range.desde); params.set('hasta', range.hasta) }\n  const result = await requestJson((range ? '/Venta/historial' : '/Venta/listar') + '?' + params, { token, signal })");
const file='src/features/sales/pages/SaleListPage.tsx', p=path.join(root,file);
let s=fs.readFileSync(p,'utf8');
s=s.replace("import { listUsers } from '../../users/api/usersApi'", "import { lastSevenSaleDays } from '../utils/saleDateRange'");
const start=s.indexOf("  const [userId, setUserId]");
const end=s.indexOf('  const [selected, setSelected]',start);
if(start<0||end<0) throw Error('state');
s=s.slice(0,start)+`  const [range, setRange] = useState(() => lastSevenSaleDays())
  const [from, setFrom] = useState(range.desde)
  const [to, setTo] = useState(range.hasta)
  const [rangeError, setRangeError] = useState('')
`+s.slice(end);
const effect=s.indexOf('  useEffect(() => {\n    if (!token || !isAdmin) return');
const next=s.indexOf('  useEffect(() => {\n    if (!token) return',effect);
if(effect<0||next<0) throw Error('effect');
s=s.slice(0,effect)+s.slice(next);
s=s.replaceAll('isAdmin && userId ? Number(userId) : undefined', 'isAdmin ? range : undefined');
s=s.replace('logout, userId, isAdmin]', 'logout, range, isAdmin]');
const filterStart=s.indexOf('      {isAdmin && <div className="sale-list-filters">');
const filterEnd=s.indexOf('      </div>}',filterStart);
if(filterStart<0||filterEnd<0) throw Error('filter');
s=s.slice(0,filterStart)+`      {isAdmin && <form className="sale-list-filters sale-date-filters" onSubmit={event => {
        event.preventDefault()
        if (!from || !to || from > to) { setRangeError('Selecciona un rango de fechas válido.'); return }
        setRangeError(''); setRange({ desde: from, hasta: to }); setPage(1); setSelected(null); setSuccess('')
      }}>
        <label>Desde<input type="date" value={from} required disabled={saving} onChange={event => setFrom(event.target.value)} /></label>
        <label>Hasta<input type="date" value={to} min={from} required disabled={saving} onChange={event => setTo(event.target.value)} /></label>
        <button type="submit" disabled={saving || loading}>Filtrar</button>
        <button type="button" disabled={saving || loading} onClick={() => {
          const dates = lastSevenSaleDays(); setFrom(dates.desde); setTo(dates.hasta); setRange(dates)
          setRangeError(''); setPage(1); setSelected(null); setSuccess('')
        }}>Últimos 7 días</button>
        {rangeError && <p role="alert">{rangeError}</p>}
      </form>}`+s.slice(filterEnd+'      </div>}'.length);
s=s.replace('aria-label="Ventas de la caja abierta"', "aria-label={isAdmin ? 'Historial de ventas por fechas' : 'Ventas de la caja abierta'}");
s=s.replace("{userId ? 'Este usuario no tiene ventas en la caja activa, o la caja está cerrada.' : 'No hay ventas para mostrar. La caja puede estar cerrada o aún no tener ventas.'}", "{isAdmin ? 'No hay ventas en el período seleccionado.' : 'No hay ventas para mostrar. La caja puede estar cerrada o aún no tener ventas.'}");
s=s.replace('Ventas registradas en la sesión de caja abierta</caption>', "{isAdmin ? 'Ventas del período seleccionado, de todas las sesiones de caja' : 'Ventas registradas en la sesión de caja abierta'}</caption>");
fs.writeFileSync(p,s);
fs.appendFileSync(path.join(root,'src/features/sales/pages/SaleListPage.css'), `
.sale-list-page .sale-date-filters label { width: min(100%, 180px); }
.sale-date-filters input { min-width: 0; }
`);
fs.appendFileSync(path.join(root,'tests/sales.test.ts'), `

import { lastSevenSaleDays } from '../src/features/sales/utils/saleDateRange.ts'
test('Últimos 7 días incluye hoy y seis días anteriores según Guatemala', () => {
  assert.deepEqual(lastSevenSaleDays(new Date('2026-10-29T18:00:00Z')), { desde: '2026-10-23', hasta: '2026-10-29' })
  assert.deepEqual(lastSevenSaleDays(new Date('2026-11-01T02:00:00Z')), { desde: '2026-10-25', hasta: '2026-10-31' })
})
`);
