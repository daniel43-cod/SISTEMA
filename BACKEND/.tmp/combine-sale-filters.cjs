const fs = require('node:fs'), path = require('node:path');
const root = path.resolve(__dirname, '../../Front-End.web/Front-End');
function edit(file,a,b){const p=path.join(root,file),s=fs.readFileSync(p,'utf8');if(!s.includes(a))throw Error(a);fs.writeFileSync(p,s.replace(a,b));}
fs.writeFileSync(path.join(root,'src/features/sales/utils/saleHistoryQuery.ts'), `export function saleHistoryQuery(page: number, range?: { desde: string; hasta: string }, userId?: number) {
  if (userId !== undefined && (!Number.isSafeInteger(userId) || userId <= 0)) throw new Error('Usuario inválido.')
  const params = new URLSearchParams({ pagina: String(page), tamanoPagina: '50' })
  if (range) {
    params.set('desde', range.desde); params.set('hasta', range.hasta)
    if (userId !== undefined) params.set('idUsuario', String(userId))
    return '/Venta/historial?' + params
  }
  return (userId === undefined ? '/Venta/listar' : '/Venta/caja-activa/usuario/' + userId) + '?' + params
}
`);
const api='src/features/sales/api/salesApi.ts';
edit(api,"import { requestJson }", "import { saleHistoryQuery } from '../utils/saleHistoryQuery'\nimport { requestJson }");
edit(api,'range?: { desde: string; hasta: string }): Promise<SaleHistory[]>', 'range?: { desde: string; hasta: string }, userId?: number): Promise<SaleHistory[]>');
edit(api,`  const params = new URLSearchParams({ pagina: String(page), tamanoPagina: '50' })
  if (range) { params.set('desde', range.desde); params.set('hasta', range.hasta) }
  const result = await requestJson((range ? '/Venta/historial' : '/Venta/listar') + '?' + params, { token, signal })`, '  const result = await requestJson(saleHistoryQuery(page, range, userId), { token, signal })');
const page='src/features/sales/pages/SaleListPage.tsx';
edit(page,"import { lastSevenSaleDays }", "import { listUsers } from '../../users/api/usersApi'\nimport { lastSevenSaleDays }");
edit(page, `  const [range, setRange] = useState(() => { const today = lastSevenSaleDays().hasta; return { desde: today, hasta: today } })
  const [selectedDate, setSelectedDate] = useState(range.desde)`, `  const [range, setRange] = useState<{ desde: string; hasta: string }>()
  const [selectedDate, setSelectedDate] = useState('')
  const [userId, setUserId] = useState('')
  const [users, setUsers] = useState<{ id: number; name: string }[]>([])
  const [usersLoading, setUsersLoading] = useState(false)
  const [usersError, setUsersError] = useState('')`);
edit(page,'  useEffect(() => {\n    if (!token) return', `  useEffect(() => {
    if (!token || !isAdmin) return
    const controller = new AbortController()
    setUsersLoading(true); setUsersError('')
    void listUsers(token, controller.signal).then(result => {
      if (!controller.signal.aborted) setUsers(result.map(u => ({ id: u.id_usuario, name: (u.nombre + ' ' + u.apellido).trim() + ' (' + u.usuario + ')' + (u.estado ? '' : ' · Inactivo') })))
    }).catch(failure => {
      if (controller.signal.aborted) return
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else setUsersError(failure instanceof Error ? failure.message : 'No se pudieron cargar los usuarios.')
    }).finally(() => { if (!controller.signal.aborted) setUsersLoading(false) })
    return () => controller.abort()
  }, [token, isAdmin, revision, logout])

  useEffect(() => {
    if (!token) return`);
// Las dos consultas (página y comprobación de siguiente) usan la misma combinación de filtros.
let p=path.join(root,page),s=fs.readFileSync(p,'utf8');
s=s.replaceAll('isAdmin ? range : undefined)', 'isAdmin ? range : undefined, isAdmin && userId ? Number(userId) : undefined)');
s=s.replace('logout, range, isAdmin]', 'logout, range, isAdmin, userId]');
s=s.replace("aria-label={isAdmin ? 'Historial de ventas por fechas' : 'Ventas de la caja abierta'}", "aria-label={isAdmin && range ? 'Historial de ventas por fechas' : 'Ventas de la caja abierta'}");
s=s.replace('max={dateLimits.hasta} required disabled={saving}', 'max={dateLimits.hasta} disabled={saving}');
s=s.replace('if (!day || day < dateLimits.desde || day > dateLimits.hasta) return', 'if (day && (day < dateLimits.desde || day > dateLimits.hasta)) return');
s=s.replace('setSelectedDate(day)\n          setRange({ desde: day, hasta: day })', "setSelectedDate(day); setUserId('')\n          setRange(day ? { desde: day, hasta: day } : undefined)");
s=s.replace('        }} /></label>\n      </div>}', `        }} /></label>
        <label>Usuario<select value={userId} disabled={saving || usersLoading} onChange={event => {
          setUserId(event.target.value); setPage(1); setSelected(null); setSuccess('')
        }}><option value="">Todos</option>{users.map(u => <option key={u.id} value={u.id}>{u.name}</option>)}</select></label>
        <button type="button" disabled={saving || loading} onClick={() => {
          setSelectedDate(''); setRange(undefined); setUserId(''); setPage(1); setSelected(null); setSuccess('')
        }}>Caja activa</button>
        {usersLoading && <span role="status">Cargando usuarios…</span>}
        {usersError && <div role="alert"><p>{usersError}</p><button type="button" disabled={saving} onClick={() => setRevision(v => v + 1)}>Reintentar usuarios</button></div>}
      </div>}`);
s=s.replace("{isAdmin ? 'No hay ventas en el período seleccionado.'", "{isAdmin && range ? 'No hay ventas para los filtros de esa fecha.'");
s=s.replace("{isAdmin ? 'Ventas del período seleccionado, de todas las sesiones de caja'", "{isAdmin && range ? 'Ventas de la fecha seleccionada, de todas las sesiones de caja'");
fs.writeFileSync(p,s);
fs.appendFileSync(path.join(root,'tests/sales.test.ts'), `

import { saleHistoryQuery } from '../src/features/sales/utils/saleHistoryQuery.ts'
test('Ventas combina caja activa o fecha con el usuario seleccionado', () => {
  assert.equal(saleHistoryQuery(1), '/Venta/listar?pagina=1&tamanoPagina=50')
  assert.equal(saleHistoryQuery(1, undefined, 2), '/Venta/caja-activa/usuario/2?pagina=1&tamanoPagina=50')
  const range = { desde: '2026-10-23', hasta: '2026-10-23' }
  assert.equal(saleHistoryQuery(1, range), '/Venta/historial?pagina=1&tamanoPagina=50&desde=2026-10-23&hasta=2026-10-23')
  assert.equal(saleHistoryQuery(2, range, 3), '/Venta/historial?pagina=2&tamanoPagina=50&desde=2026-10-23&hasta=2026-10-23&idUsuario=3')
})
`);
