export function buildExpense(description: string, amount: string, note: string) {
  const descripcion = description.trim()
  const observacion = note.trim()
  if (!descripcion || description.length > 300) throw new Error('Ingresa una descripción de hasta 300 caracteres.')
  if (note.length > 500) throw new Error('La observación admite hasta 500 caracteres.')
  const normalized = amount.trim().replace(',', '.')
  if (!/^\d{1,8}(?:\.\d{1,2})?$/.test(normalized)) throw new Error('Ingresa un monto válido con hasta dos decimales.')
  const [whole, decimals = ''] = normalized.split('.')
  const cents = Number(whole) * 100 + Number(decimals.padEnd(2, '0'))
  if (cents < 1 || cents > 9999999999) throw new Error('El monto debe estar entre Q0.01 y Q99,999,999.99.')
  return { descripcion, monto: cents / 100, observacion: observacion || null }
}
