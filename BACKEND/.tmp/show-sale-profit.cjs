const fs = require('node:fs'), path = require('node:path');
const root = path.resolve(__dirname, '../../Front-End.web/Front-End');
function edit(file, pairs) {
 const target = path.join(root, file); let s = fs.readFileSync(target, 'utf8');
 for (const [a,b] of pairs) { if (!s.includes(a)) throw Error(file + ': ' + a); s = s.replace(a,b); }
 fs.writeFileSync(target, s);
}
edit('src/features/sales/api/salesApi.ts', [
 ['total: number; saldo_pendiente:', 'total: number; ganancia_total?: number; saldo_pendiente:'],
 ["amount(v.saldo_pendiente) && typeof v.estado", "amount(v.saldo_pendiente) && (v.ganancia_total === undefined || (typeof v.ganancia_total === 'number' && Number.isFinite(v.ganancia_total))) && typeof v.estado"],
]);
edit('src/features/sales/pages/SaleListPage.tsx', [
 ['  const token = session?.token', "  const token = session?.token\n  const isAdmin = session?.user.role === 'ADMINISTRADOR'"],
 ['<th scope="col">Total</th>', '<th scope="col" className="sale-list-money">Total</th>{isAdmin && <th scope="col" className="sale-list-money">Ganancia</th>}'],
 ['<th scope="col">Saldo pendiente</th>', '<th scope="col" className="sale-list-money">Saldo pendiente</th>'],
 ['{money.format(sale.total)}</td>', "{money.format(sale.total)}</td>\n                  {isAdmin && <td className=\"sale-list-money\">{sale.ganancia_total === undefined ? 'No disponible' : money.format(sale.ganancia_total)}</td>}"],
]);
edit('src/features/sales/pages/SalePaymentForm.tsx', [
 ['<div><dt>Total</dt><dd>{money.format(sale.total)}</dd></div>', `<div><dt>Total</dt><dd>{money.format(sale.total)}</dd></div>
      {session?.user.role === 'ADMINISTRADOR' && <div><dt>Ganancia</dt><dd>{sale.ganancia_total === undefined ? 'No disponible' : money.format(sale.ganancia_total)}</dd></div>}`],
]);
edit('src/features/sales/pages/SaleListPage.css', [
 ['.sale-list-page th:nth-child(3), .sale-list-page th:nth-child(4), .sale-list-money', '.sale-list-money'],
]);
