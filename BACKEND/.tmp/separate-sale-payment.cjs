const fs=require('node:fs'),path=require('node:path');
const root=path.resolve(__dirname,'../../Front-End.web/Front-End');
const p=path.join(root,'src/features/sales/pages/SalePaymentForm.tsx');let s=fs.readFileSync(p,'utf8');
s=s.replace('  const heading = useRef', '  const paymentDialog = useRef<HTMLDialogElement>(null)\n  const paymentButton = useRef<HTMLButtonElement>(null)\n  const paymentAmount = useRef<HTMLInputElement>(null)\n  const [paymentOpen, setPaymentOpen] = useState(false)\n  const heading = useRef');
const anchor='  async function submit(event: FormEvent<HTMLFormElement>)';
s=s.replace(anchor, `  useEffect(() => {
    if (!paymentOpen) return
    const panel = paymentDialog.current
    panel?.showModal(); paymentAmount.current?.focus()
    return () => { panel?.close(); paymentButton.current?.focus() }
  }, [paymentOpen])

`+anchor);
s=s.replace('return createPortal(<dialog ref={dialog}', 'return createPortal(<><dialog ref={dialog}');
const badge="<span className={sale.saldo_pendiente === 0 ? 'sale-list-status is-paid' : 'sale-list-status'}>{sale.saldo_pendiente === 0 ? 'Pagada' : sale.estado}</span>";
if(!s.includes(badge))throw Error('badge');
s=s.replace(badge, `<span className="sale-detail-payment-action">`+badge+`
          {sale.saldo_pendiente > 0 && <button ref={paymentButton} type="button" disabled={saving || loading || !!detailError}
            onClick={() => setPaymentOpen(true)} aria-haspopup="dialog">Abonar</button>}
        </span>`);
const start=s.indexOf('    {sale.saldo_pendiente > 0 ? <form onSubmit={submit}>');
const end=s.indexOf('    </form> : null}',start);
if(start<0||end<0)throw Error('form');
let form=s.slice(start,end+'    </form> : null}'.length);
form=form.replace('    {sale.saldo_pendiente > 0 ? <form onSubmit={submit}>','    <form onSubmit={submit}>').replace('    </form> : null}','    </form>');
form=form.replace('<input inputMode="decimal"', '<input ref={paymentAmount} inputMode="decimal"');
s=s.slice(0,start)+s.slice(end+'    </form> : null}'.length);
s=s.replace('  </dialog>, document.body)', `  </dialog>
  {paymentOpen && <dialog ref={paymentDialog} className="sales-page sale-list-page sale-payment-dialog sale-abono-dialog"
    aria-labelledby="sale-abono-title" onCancel={event => { event.preventDefault(); if (!busy.current) setPaymentOpen(false) }}>
    <section className="sales-card sale-payment">
      <div className="sale-detail-title-row"><h2 id="sale-abono-title">Abonar · {sale.cliente}</h2>
        <button type="button" className="sale-detail-close" disabled={saving} onClick={() => setPaymentOpen(false)} aria-label="Cerrar formulario de abono">×</button>
      </div>
      <p>Saldo pendiente: <strong>{money.format(sale.saldo_pendiente)}</strong></p>
`+form+`
    </section>
  </dialog>}
  </>, document.body)`);
fs.writeFileSync(p,s);
fs.appendFileSync(path.join(root,'src/features/sales/pages/SaleListPage.css'), `
.sale-detail-payment-action { display: inline-flex; align-items: center; gap: 12px; }
.sale-abono-dialog { width: min(520px, calc(100vw - 32px)); }
`);
