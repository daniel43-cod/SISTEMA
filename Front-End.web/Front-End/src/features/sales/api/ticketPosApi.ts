import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'

export type TicketPerson = { nombre: string; razonSocial: string | null; identificacion: string | null; direccion: string | null }
export type TicketLine = { descripcion: string; cantidad: number; unidad: string; precioUnitario: number; descuento: number; total: number }
export type TicketCertificate = { uuid: string; serie: string; numero: string; fecha: string; nombreCertificador: string; nitCertificador: string; qrUrl: string }
export type TicketPos = { idVenta: number; tipo: 'FACTURA_FEL' | 'COMPROBANTE_INTERNO'; ambiente: string | null; fecha: string; vendedor: string; emisor: TicketPerson; receptor: TicketPerson; lineas: TicketLine[]; subtotal: number; descuento: number; iva: number | null; total: number; certificacion: TicketCertificate | null; frases: string[]; avisos: string[] }
const obj = (v: unknown): v is Record<string, unknown> => !!v && typeof v === 'object' && !Array.isArray(v)
const amount = (v: unknown): v is number => typeof v === 'number' && Number.isFinite(v) && v >= 0
const nullableText = (v: unknown) => v === null || typeof v === 'string'
const person = (v: unknown) => obj(v) && typeof v.nombre === 'string' && ['razonSocial','identificacion','direccion'].every(k => nullableText(v[k]))
export function parseTicketPos(value: unknown, saleId: number): TicketPos {
  const invalid = () => new ApiError('No se pudo interpretar el ticket. No se envió nada a la impresora.')
  if (!obj(value) || value.idVenta !== saleId || !['FACTURA_FEL','COMPROBANTE_INTERNO'].includes(String(value.tipo)) ||
      !nullableText(value.ambiente) || typeof value.fecha !== 'string' || !Number.isFinite(Date.parse(value.fecha)) || typeof value.vendedor !== 'string' ||
      !person(value.emisor) || !person(value.receptor) || !['subtotal','descuento','total'].every(k => amount(value[k])) || !(value.iva === null || amount(value.iva)) ||
      !Array.isArray(value.lineas) || !value.lineas.length || !value.lineas.every(l => obj(l) && typeof l.descripcion === 'string' && typeof l.unidad === 'string' &&
        amount(l.cantidad) && l.cantidad > 0 && ['precioUnitario','descuento','total'].every(k => amount(l[k]))) ||
      !Array.isArray(value.frases) || !value.frases.every(s => typeof s === 'string') || !Array.isArray(value.avisos) || !value.avisos.every(s => typeof s === 'string')) throw invalid()
  if (value.tipo === 'FACTURA_FEL') {
    const c = value.certificacion
    if (!['PRUEBAS','PRODUCCION'].includes(String(value.ambiente)) || !obj(c) || !['uuid','serie','numero','fecha','nombreCertificador','nitCertificador','qrUrl'].every(k => typeof c[k] === 'string' && !!c[k]) || !Number.isFinite(Date.parse(c.fecha as string))) throw invalid()
    let url: URL
    try { url = new URL(c.qrUrl as string) } catch { throw invalid() }
    const issuer = value.emisor as TicketPerson
    if (url.origin !== 'https://portal.sat.gob.gt' || url.pathname !== '/portal/verificador-integrado/' || url.username || url.password || url.hash ||
        url.searchParams.get('tipo') !== 'autorizacion' || url.searchParams.get('documento') !== 'FEL' || url.searchParams.get('emisor') !== issuer.identificacion ||
        url.searchParams.get('serie') !== c.serie || url.searchParams.get('numero') !== c.numero) throw invalid()
  } else if (value.certificacion !== null || value.ambiente !== null) throw invalid()
  return value as unknown as TicketPos
}
export async function loadTicketPos(saleId: number, token: string, signal?: AbortSignal) {
  if (!Number.isSafeInteger(saleId) || saleId < 1) throw new ApiError('Venta inválida.')
  return parseTicketPos(await requestJson('/comprobantes/ventas/' + saleId + '/ticket-pos', {token,signal,validationMessages:true}),saleId)
}
