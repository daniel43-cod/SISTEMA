export type ReceiverType = '' | 'CF' | 'NIT' | 'CUI'

export function receiverDocumentInput(type: ReceiverType, value: string): string {
  return type === 'CUI' ? value.replace(/[^0-9]/g, '').slice(0, 13)
    : type === 'NIT' ? value.toUpperCase().replace(/[^0-9K]/g, '').slice(0, 20) : ''
}

export function saleReceiver(type: ReceiverType, document: string, name: string, totalCents: number) {
  if (!type) throw new Error('Selecciona el tipo de venta: consumidor final, CUI o NIT.')
  if (type === 'CF') {
    if (totalCents >= 250000) throw new Error('Para Q2,500.00 o más debes seleccionar NIT o CUI.')
    return { tipo_identificacion_receptor: type, identificacion_receptor: 'CF', nombre_receptor: 'CONSUMIDOR FINAL' }
  }
  const id = document.trim().toUpperCase()
  if (type === 'CUI' && !/^[0-9]{13}$/.test(id)) throw new Error('El CUI debe contener exactamente 13 números.')
  if (type === 'NIT' && !/^[0-9]+[0-9K]$/.test(id)) throw new Error('Ingresa un NIT válido, con K al final si corresponde.')
  if (!name.trim()) throw new Error('Ingresa el nombre fiscal del comprador.')
  return { tipo_identificacion_receptor: type, identificacion_receptor: id, nombre_receptor: name.trim() }
}
