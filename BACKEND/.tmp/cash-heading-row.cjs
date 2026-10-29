const fs = require('fs');
const root = 'C:/Users/coy81/Desktop/SISTEMA/SISTEMA/Front-End.web/Front-End/src/features/cash/pages/';
const path = root + 'CashPage.tsx';
let s = fs.readFileSync(path, 'utf8');
s = s.replace('<div className="cash-heading">', '<div className="cash-heading">\n      {current && phase === \'ready\' && <h3 id="cash-movements-title">Movimientos del turno</h3>}');
s = s.replace('          <h3 id="cash-movements-title">Movimientos del turno</h3>\n', '');
fs.writeFileSync(path, s);
fs.appendFileSync(root + 'CashPage.css', '\n.staff-shell-cash .cash-heading { position: static; display: flex; justify-content: flex-end; align-items: center; width: 100%; }\n.staff-shell-cash .cash-heading h3 { margin: 0 auto 0 0; font-size: 1rem; }\n');
