const fs = require('node:fs'), path = require('node:path');
const root = path.resolve(__dirname, '../../Front-End.web/Front-End');
function edit(file, pairs) {
 const target = path.join(root, file); let s = fs.readFileSync(target, 'utf8');
 for (const [before, after] of pairs) { if (!s.includes(before)) throw Error(before); s = s.replace(before, after); }
 fs.writeFileSync(target, s);
}
edit('src/features/sales/pages/SalePaymentForm.tsx', [
 ['{ sale, onClose, onPaid }: { sale: SaleHistory; onClose: () => void; onPaid: () => void }', '{ sale, onClose, onPaid, onSaving }: { sale: SaleHistory; onClose: () => void; onPaid: () => void; onSaving: (value: boolean) => void }'],
 ['busy.current = true; setSaving(true)', 'busy.current = true; setSaving(true); onSaving(true)'],
 ['finally { busy.current = false; if (alive.current) setSaving(false) }', 'finally { busy.current = false; onSaving(false); if (alive.current) setSaving(false) }'],
 ]);
edit('src/features/sales/pages/SaleListPage.tsx', [
 ["  const [success, setSuccess]", "  const [saving, setSaving] = useState(false)\n  const [success, setSuccess]"],
 ['key={selected.id_ventas} sale={selected}', 'key={selected.id_ventas} sale={selected} onSaving={setSaving}'],
 ['label="Actualizar ventas" loading={loading}', 'label="Actualizar ventas" loading={loading || saving}'],
 ["onClick={() => { setSelected(sale); setSuccess('') }}", "onClick={() => { if (!saving) { setSelected(sale); setSuccess('') } }}"],
 ['<td><button type="button" aria-label=', '<td><button type="button" disabled={saving} aria-label='],
 ['disabled={loading || page === 1}', 'disabled={loading || saving || page === 1}'],
 ['disabled={loading || !!error || !hasNext}', 'disabled={loading || saving || !!error || !hasNext}'],
 ]);
