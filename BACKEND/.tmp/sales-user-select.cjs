const fs = require('node:fs'), path = require('node:path');
const root = path.resolve(__dirname, '../../Front-End.web/Front-End');
function edit(file, a, b) { const p = path.join(root, file), s = fs.readFileSync(p, 'utf8'); if (!s.includes(a)) throw Error(a); fs.writeFileSync(p, s.replace(a,b)); }
edit('src/features/sales/api/salesApi.ts', 'signal?: AbortSignal, page = 1): Promise<SaleHistory[]>', 'signal?: AbortSignal, page = 1, userId?: number): Promise<SaleHistory[]>');
edit('src/features/sales/api/salesApi.ts', 'const result = await requestJson(`/Venta/listar?pagina=${page}&tamanoPagina=50`, { token, signal })', "const route = userId === undefined ? '/Venta/listar' : '/Venta/caja-activa/usuario/' + userId\n  const result = await requestJson(route + '?pagina=' + page + '&tamanoPagina=50', { token, signal })");
const page = 'src/features/sales/pages/SaleListPage.tsx';
edit(page, "import { SalePaymentForm }", "import { listUsers } from '../../users/api/usersApi'\nimport { SalePaymentForm }");
edit(page, '  const [selected, setSelected]', `  const [userId, setUserId] = useState('')
  const [users, setUsers] = useState<{ id: number; name: string; active: boolean }[]>([])
  const [usersLoading, setUsersLoading] = useState(false)
  const [usersError, setUsersError] = useState('')
  const [selected, setSelected]`);
edit(page, '  useEffect(() => {\n    if (!token) return', `  useEffect(() => {
    if (!token || !isAdmin) return
    const controller = new AbortController()
    setUsersLoading(true); setUsersError('')
    void listUsers(token, controller.signal).then(result => {
      if (!controller.signal.aborted) setUsers(result.map(user => ({ id: user.id_usuario,
        name: (user.nombre + ' ' + user.apellido).trim() + ' (' + user.usuario + ')', active: user.estado })))
    }).catch(failure => {
      if (controller.signal.aborted) return
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else setUsersError(failure instanceof Error ? failure.message : 'No se pudieron cargar los usuarios.')
    }).finally(() => { if (!controller.signal.aborted) setUsersLoading(false) })
    return () => controller.abort()
  }, [token, isAdmin, revision, logout])

  useEffect(() => {
    if (!token) return`);
edit(page, 'controller.signal, page)', 'controller.signal, page, isAdmin && userId ? Number(userId) : undefined)');
edit(page, 'controller.signal, page + 1)', 'controller.signal, page + 1, isAdmin && userId ? Number(userId) : undefined)');
edit(page, '}, [token, page, revision, logout])', '}, [token, page, revision, logout, userId, isAdmin])');
edit(page, '      {error && <div role="alert"', `      {isAdmin && <div className="sale-list-filters">
        <label>Responsable
          <select value={userId} disabled={saving || usersLoading} onChange={event => { setUserId(event.target.value); setPage(1); setSelected(null); setSuccess('') }}>
            <option value="">Todos los usuarios</option>
            {users.map(user => <option key={user.id} value={user.id}>{user.name}{user.active ? '' : ' · Inactivo'}</option>)}
          </select>
        </label>
        {usersLoading && <span role="status">Cargando usuarios…</span>}
        {usersError && <div role="alert"><p>{usersError}</p><button type="button" disabled={saving} onClick={() => setRevision(v => v + 1)}>Reintentar usuarios</button></div>}
      </div>}
      {error && <div role="alert"`);
edit(page, 'No hay ventas para mostrar. La caja puede estar cerrada o aún no tener ventas.', "{userId ? 'Este usuario no tiene ventas en la caja activa, o la caja está cerrada.' : 'No hay ventas para mostrar. La caja puede estar cerrada o aún no tener ventas.'}");
fs.appendFileSync(path.join(root, 'src/features/sales/pages/SaleListPage.css'), `
.sale-list-filters { display: flex; align-items: end; flex-wrap: wrap; gap: 12px; margin: 12px 0 20px; }
.sale-list-filters label { width: min(100%, 360px); }
`);
