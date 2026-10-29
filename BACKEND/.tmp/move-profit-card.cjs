const fs = require('fs');
const root = 'C:/Users/coy81/Desktop/SISTEMA/SISTEMA/Front-End.web/Front-End';
const path = root + '/src/features/sales/pages/SaleListPage.tsx';
let text = fs.readFileSync(path, 'utf8');
const start = text.indexOf('{isAdmin && rows.length === 0 && <div className="sale-profit-card">');
const end = text.indexOf('<div className="sale-list-body"', start);
if (start < 0 || end < 0) throw Error('Missing card');
let card = text.slice(start,end).trim().replace('{isAdmin && rows.length === 0 && ', '').replace(/}\s*$/, '');
card = card.replace('          <span>Ganancia total</span>\n', '');
text = text.slice(0,start) + text.slice(end);
const headStart = text.indexOf('<th scope="col" className="sale-list-money sale-profit-column">');
const headEnd = text.indexOf('</th>', headStart) + 5;
if (headStart < 0 || headEnd < 5) throw Error('Missing gain header');
text = text.slice(0,headStart) + '<th scope="col" className="sale-list-money">Ganancia</th>' + text.slice(headEnd);
const anchor = '<div className="sale-date-actions"><RefreshButton';
if (!text.includes(anchor)) throw Error('Missing actions');
text = text.replace(anchor, '<div className="sale-date-actions"><div className="sale-profit-summary"><span>Ganancia total</span>' + card + '</div><RefreshButton');
fs.writeFileSync(path,text);
const cssPath = root + '/src/features/sales/pages/SaleListPage.css';
let css = fs.readFileSync(cssPath,'utf8');
const cssStart = css.indexOf('.sale-profit-card {');
if(cssStart < 0) throw Error('Missing CSS');
css = css.slice(0,cssStart) + `.sale-profit-summary { display: flex; align-items: center; gap: 8px; flex-shrink: 0; }
.sale-profit-summary > span { font-size: .85rem; color: var(--text-muted); white-space: nowrap; }
.sale-profit-card { padding: 2px 8px; border: 1px solid var(--border); border-radius: 8px; background: #eff8ff; }
.sale-profit-card > div { display: flex; align-items: center; gap: 8px; }
.sale-profit-card strong { font-size: .9rem; font-weight: 500; font-variant-numeric: tabular-nums; white-space: nowrap; }
.sale-list-view button.sale-profit-toggle { display: inline-flex; align-items: center; justify-content: center; width: 32px; min-height: 32px; padding: 4px; border: 0; background: transparent; }
@media (max-width: 1100px) { .sale-list-view .sale-date-filters { flex-wrap: wrap; } .sale-date-actions { flex-wrap: wrap; } }
`;
fs.writeFileSync(cssPath,css);

