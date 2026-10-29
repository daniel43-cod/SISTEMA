const fs=require('fs');const root='C:/Users/coy81/Desktop/SISTEMA/SISTEMA/Front-End.web/Front-End/src/features/purchases/';let p=root+'api/purchasesApi.ts';let s=fs.readFileSync(p,'utf8').replace('payment: number, note: string)', 'payment: number, note: string, operationId: string)').replace('body: { id_compra: purchaseId, monto: payment, observacion:', 'body: { idOperacion: operationId, id_compra: purchaseId, monto: payment, observacion:');fs.writeFileSync(p,s);
p=root+'components/PurchaseDetails.tsx';s=fs.readFileSync(p,'utf8');s=s.replace("  const [payment, setPayment] = useState('')",`  const storageKey = 'purchase-payment-' + purchase.id_compra
  const [pending] = useState<{ id: string; payment: string; note: string } | null>(() => {
    try {
      const saved = JSON.parse(sessionStorage.getItem(storageKey) || 'null')
      return saved && typeof saved.id === 'string' && typeof saved.payment === 'string' && typeof saved.note === 'string' ? saved : null
    } catch { return null }
  })
  const operation = useRef(pending)
  const [payment, setPayment] = useState(pending?.payment ?? '')`);
s=s.replace("const [note, setNote] = useState('')", "const [note, setNote] = useState(pending?.note ?? '')");s=s.replace('useState(false)\n  const busy', 'useState(!!pending)\n  const busy');s=s.replace('busy.current || uncertain || !canPay','busy.current || (!uncertain && !canPay)');s=s.replace('cents > Math.round(purchase.saldo_pendiente * 100)', '(!uncertain && cents > Math.round(purchase.saldo_pendiente * 100))');
s=s.replace('    busy.current = true; setSaving(true); setError(\'\')', `    if (!operation.current) operation.current = { id: crypto.randomUUID(), payment, note }
    try { sessionStorage.setItem(storageKey, JSON.stringify(operation.current)) }
    catch { setError('No se pudo conservar la operación para reintentos. Habilita el almacenamiento del navegador.'); return }
    busy.current = true; setSaving(true); setError('')`);
s=s.replace('await payPurchase(token, purchase.id_compra, cents / 100, note); onPaid()', 'await payPurchase(token, purchase.id_compra, cents / 100, note, operation.current.id); sessionStorage.removeItem(storageKey); operation.current = null; onPaid()');
s=s.replace('        setUncertain(ambiguous)', `        setUncertain(true)`);
s=s.replace('No se pudo confirmar el abono. Cierra este detalle y actualiza el historial antes de volver a pagar.', 'No se pudo confirmar el abono. Reintenta con los mismos datos; no se registrará dos veces.');
s=s.replace('{purchase.saldo_pendiente > 0 && <form', '{(purchase.saldo_pendiente > 0 || uncertain) && <form');s=s.replace('disabled={saving || !canPay || uncertain}', 'disabled={saving || (!canPay && !uncertain)}');s=s.replace('placeholder="Ej. 250.00"', 'readOnly={uncertain} placeholder="Ej. 250.00"');s=s.replace('maxLength={100} placeholder="Ej. Abono', 'readOnly={uncertain} maxLength={100} placeholder="Ej. Abono');s=s.replace("{saving ? 'Guardando…' : 'Registrar abono'}", "{saving ? 'Guardando…' : uncertain ? 'Reintentar abono' : 'Registrar abono'}");fs.writeFileSync(p,s);
