const fs = require('node:fs'), path = require('node:path');
const root = path.resolve(__dirname, '../../Front-End.web/Front-End');
const file = path.join(root, 'src/features/sales/pages/SalePaymentForm.tsx');
let s = fs.readFileSync(file, 'utf8');
function replace(a,b) { if (!s.includes(a)) throw Error(a); s = s.replace(a,b); }
replace("import { useEffect, useRef, useState }", "import { createPortal } from 'react-dom'\nimport { useEffect, useRef, useState }");
replace('  const heading = useRef', '  const dialog = useRef<HTMLDialogElement>(null)\n  const heading = useRef');
replace('    alive.current = true\n    heading.current?.focus()', "    alive.current = true\n    const panel = dialog.current\n    const previousOverflow = document.body.style.overflow\n    const previousFocus = document.activeElement\n    panel?.showModal()\n    document.body.style.overflow = 'hidden'\n    heading.current?.focus()");
replace("    heading.current?.scrollIntoView({ behavior: 'smooth', block: 'start' })\n    return () => { alive.current = false }", "    return () => {\n      alive.current = false\n      panel?.close()\n      document.body.style.overflow = previousOverflow\n      if (previousFocus instanceof HTMLElement && previousFocus.isConnected) previousFocus.focus()\n    }");
replace('  return <section className="sales-card sale-payment" aria-labelledby="sale-payment-title">', '  return createPortal(<dialog ref={dialog} className="sales-page sale-list-page sale-payment-dialog" aria-labelledby="sale-payment-title"\n    onCancel={event => { event.preventDefault(); if (!busy.current) onClose() }}>\n    <section className="sales-card sale-payment">');
replace('  </section>\n}', '    </section>\n  </dialog>, document.body)\n}');
fs.writeFileSync(file, s);
fs.appendFileSync(path.join(root, 'src/features/sales/pages/SaleListPage.css'), `
.sale-payment-dialog { width: min(900px, calc(100vw - 32px)); max-height: calc(100dvh - 48px); margin: auto; padding: 0; overflow: auto; border: 0; border-radius: var(--radius); background: var(--surface); color: var(--text); }
.sale-payment-dialog:not([open]) { display: none; }
.sale-payment-dialog::backdrop { background: #0f172a80; }
.sale-payment-dialog .sale-payment { border: 0; }
@media (max-width: 520px) { .sale-payment-dialog { width: calc(100vw - 24px); max-height: calc(100dvh - 24px); } }
`);
