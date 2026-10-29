const fs=require('fs');
const root='C:/Users/coy81/Desktop/SISTEMA/SISTEMA/Front-End.web/Front-End/src/features/cash/pages/';
const p=root+'CashPage.tsx';let s=fs.readFileSync(p,'utf8');
s=s.replace('  const busy = useRef(false)', `  const closingDialog = useRef<HTMLDialogElement>(null)
  useEffect(() => {
    if (!closing || phase !== 'ready' || !current) return
    const dialog = closingDialog.current
    const previousFocus = document.activeElement
    const overflow = document.body.style.overflow
    dialog?.showModal()
    document.body.style.overflow = 'hidden'
    return () => {
      dialog?.close()
      document.body.style.overflow = overflow
      if (previousFocus instanceof HTMLElement && previousFocus.isConnected) previousFocus.focus()
    }
  }, [closing, phase, current])
  const busy = useRef(false)`);
s=s.replace('{error && <p className="cash-error" role="alert">{error}</p>}', '{error && !closing && <p className="cash-error" role="alert">{error}</p>}');
s=s.replace('{closing &&\n          <form', `{closing && createPortal(<dialog ref={closingDialog} className="cash-closing-dialog" aria-labelledby="cash-closing-title"
          onCancel={event => { event.preventDefault(); if (!busy.current) { setClosing(false); setReview(null); setError('') } }}>
          <button type="button" className="cash-dialog-close" aria-label="Cerrar formulario" disabled={saving}
            onClick={() => { setClosing(false); setReview(null); setError('') }}>×</button>
          <form`);
s=s.replace('<h3>Cierre del turno</h3>', '<h3 id="cash-closing-title">Cierre del turno</h3>\n            {error && <p className="cash-error" role="alert">{error}</p>}');
s=s.replace('          </form>}\n      </> : <>', '          </form></dialog>, document.body)}\n      </> : <>');
fs.writeFileSync(p,s);
fs.appendFileSync(root+'CashPage.css', `
.cash-closing-dialog { width: min(420px, calc(100vw - 32px)); box-sizing: border-box; max-height: calc(100dvh - 40px); overflow: auto; margin: auto; padding: 24px; border: 0; border-radius: 14px; background: #fff; color: #263747; }
.cash-closing-dialog::backdrop { background: #0f172a80; }
.cash-closing-dialog:not([open]) { display: none; }
.cash-closing-dialog .cash-closing-form { border: 0; padding-top: 0; gap: 14px; }
.cash-closing-dialog .cash-form fieldset { gap: 14px; }
.cash-closing-dialog .cash-hint { margin: 0; font-size: .875rem; }
.cash-closing-dialog .cash-dialog-close { display: block; margin: -8px -8px 0 auto; width: 32px; height: 32px; border: 0; background: transparent; font-size: 1.5rem; cursor: pointer; }
`);
