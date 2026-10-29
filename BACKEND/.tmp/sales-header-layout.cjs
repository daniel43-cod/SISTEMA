const fs=require('fs');const root='C:/Users/coy81/Desktop/SISTEMA/SISTEMA/Front-End.web/Front-End/src/features/sales/pages/';const p=root+'SaleListPage.tsx';let s=fs.readFileSync(p,'utf8');s=s.replace('  const [salesTotal,','  const [salesVisible, setSalesVisible] = useState(false)\n  const [salesTotal,');
const a=s.indexOf('      {isAdmin && <div className="sale-list-filters');const b=s.indexOf('        {!isAdmin &&',a);const old=s.slice(a,b);
const user=old.slice(old.indexOf('<label>Usuario'),old.indexOf('</select></label>')+17);
const dateStart=old.indexOf('<label className="sale-filter-date">');const dateEnd=old.indexOf('</label>',dateStart)+8;const date=old.slice(dateStart,dateEnd);
const buttonStart=old.indexOf('<button type="button" disabled={saving || loading}');const buttonEnd=old.indexOf('</button>',buttonStart)+9;const button=old.slice(buttonStart,buttonEnd);
const refresh=old.slice(old.indexOf('<RefreshButton'),old.indexOf(' />',old.indexOf('<RefreshButton'))+3);
const errors=old.slice(old.indexOf('{usersLoading'),old.lastIndexOf('</div>}'));
const summary=(label,visible,setter,value,hidden)=>`<div className="sale-profit-summary"><span>${label}</span><div className="sale-profit-card"><div>
<strong aria-label={${visible} ? undefined : '${hidden}'}>{${visible} ? (${value} === null ? '—' : money.format(${value})) : 'Q ••••••'}</strong>
<button type="button" className="sale-profit-toggle" aria-label={${visible} ? 'Ocultar ${label.toLowerCase()}' : 'Mostrar ${label.toLowerCase()}'} title={${visible} ? 'Ocultar' : 'Mostrar'} aria-pressed={${visible}} onClick={() => ${setter}(v => !v)}>
<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12Z" /><circle cx="12" cy="12" r="3" />{!${visible} && <path d="m3 3 18 18" />}</svg></button>
</div></div></div>`;
s=s.slice(0,a)+`      {isAdmin && <div className="sale-list-filters sale-date-filters sale-arranged-filters">
<div className="sale-filter-stack"><div className="sale-user-cash-row">${user}${button}</div>
<div className="sale-date-row">${date}${refresh}</div></div>
<div className="sale-totals-stack">${summary('Total de ventas','salesVisible','setSalesVisible','salesTotal','Total de ventas oculto')}${summary('Ganancia total','profitVisible','setProfitVisible','profitTotal','Ganancia oculta')}</div>
${errors}</div>}
`+s.slice(b);fs.writeFileSync(p,s);
fs.appendFileSync(root+'SaleListPage.css', `
.sale-list-view .sale-arranged-filters { align-items: flex-start; justify-content: flex-start; gap: 20px 32px; flex-wrap: wrap; }
.sale-filter-stack { display: grid; gap: 12px; }
.sale-user-cash-row, .sale-date-row { display: flex; align-items: flex-end; gap: 12px; flex-wrap: wrap; }
.sale-list-view .sale-arranged-filters .sale-filter-date { margin-left: 0; order: 0; }
.sale-totals-stack { display: grid; gap: 8px; align-self: flex-start; }
.sale-totals-stack .sale-profit-summary { display: grid; grid-template-columns: 110px minmax(160px, auto); gap: 8px; }
.sale-totals-stack .sale-profit-card { min-height: 38px; box-sizing: border-box; }
.sale-totals-stack .sale-profit-card > div { justify-content: space-between; }
`);
