const fs=require('node:fs'),path=require('node:path');
const root=path.resolve(__dirname,'../../Front-End.web/Front-End');
const layout=path.join(root,'src/app/layouts/StaffLayout.tsx');let s=fs.readFileSync(layout,'utf8');
s="import { createPortal } from 'react-dom'\n"+s;
s=s.replace('  const accountPanelId = useId()', '  const accountPanelId = useId()\n  const [saleAccountTarget, setSaleAccountTarget] = useState<HTMLDivElement | null>(null)');
const start=s.indexOf('          <div className="staff-account" ref={accountRef}');
const end=s.indexOf('        </header>',start);
if(start<0||end<0)throw Error('account block');
const account=s.slice(start,end).trim();
s=s.slice(0,start)+`          {current.id === 'listado-ventas' && saleAccountTarget ? createPortal(accountControls, saleAccountTarget) : accountControls}
`+s.slice(end);
s=s.replace('  return (\n    <div className={`staff-shell', '  const accountControls = (\n' + account + '\n  )\n\n  return (\n    <div className={`staff-shell');
s=s.replace('<SaleListPage key={session.user.id} />', '<SaleListPage key={session.user.id} onAccountTarget={setSaleAccountTarget} />');
fs.writeFileSync(layout,s);
const file=path.join(root,'src/features/sales/pages/SaleListPage.tsx');s=fs.readFileSync(file,'utf8');
s=s.replace('export function SaleListPage() {','export function SaleListPage({ onAccountTarget }: { onAccountTarget?: (node: HTMLDivElement | null) => void } = {}) {');
s=s.replace('    <div className="sales-card">', '    <div className="sales-card">\n      {onAccountTarget && <div className="sale-list-account-row"><div ref={onAccountTarget} /></div>}');
fs.writeFileSync(file,s);
fs.appendFileSync(path.join(root,'src/features/sales/pages/SaleListPage.css'), `
.sale-list-account-row { display: flex; justify-content: flex-end; flex-shrink: 0; margin-bottom: 8px; }
.sale-list-view > .sales-card { overflow: visible; }
.sale-list-account-row .staff-account-panel { z-index: 30; }
`);
fs.appendFileSync(path.join(root,'src/app/layouts/StaffLayout.css'), `
.staff-shell-sale-list .staff-content { padding-top: 6px; }
@media (min-width: 801px) { .staff-shell-sale-list .staff-header { display: none; } }
`);
