import type { TicketPos } from '../api/ticketPosApi.ts'

export type PaperWidth = 58 | 80
export const paperWidth = (value: unknown): PaperWidth => value === 58 || value === '58' ? 58 : 80
const escape = (v: unknown) => String(v ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]!))
const money = (v: number) => new Intl.NumberFormat('es-GT',{minimumFractionDigits:2,maximumFractionDigits:2}).format(v)
const date = (v: string) => new Intl.DateTimeFormat('es-GT',{timeZone:'America/Guatemala',dateStyle:'short',timeStyle:'short'}).format(new Date(v.endsWith('Z') || /[+-]\d{2}:\d{2}$/.test(v) ? v : v+'-06:00'))
export function ticketPosHtml(ticket: TicketPos, width: PaperWidth, qrDataUrl?: string): string {
  if (width !== 58 && width !== 80) throw new Error('Ancho de papel inválido.')
  if (ticket.certificacion && !qrDataUrl?.startsWith('data:image/png;base64,')) throw new Error('Falta el QR para imprimir el DTE.')
  const p = (s: unknown) => '<p>'+escape(s)+'</p>'
  const c = ticket.certificacion
  const fiscal = ticket.tipo === 'FACTURA_FEL'
  const warning = fiscal ? ticket.ambiente === 'PRUEBAS' ? 'DOCUMENTO DE PRUEBAS - SIN VALIDEZ FISCAL' : '' : 'COMPROBANTE INTERNO - NO ES FACTURA FEL'
  return '<!doctype html><html lang="es"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width"><meta http-equiv="Content-Security-Policy" content="default-src &#39;none&#39;; img-src data:; style-src &#39;unsafe-inline&#39;; base-uri &#39;none&#39;; form-action &#39;none&#39;"><title>Ticket de venta '+ticket.idVenta+'</title><style>'+
    '@page{size:auto;margin:0}*{box-sizing:border-box}html,body{margin:0;padding:0;background:#fff;color:#000}body{width:'+width+'mm;padding:3mm;font:11px Arial,sans-serif;line-height:1.35;overflow-wrap:anywhere}h1,h2,p{margin:0 0 3px}h1{font-size:15px}h2{font-size:12px}.center{text-align:center}.warning{border:1px solid #000;padding:4px;font-weight:bold;margin:6px 0}section{border-top:1px dashed #000;margin-top:7px;padding-top:7px}table{width:100%;border-collapse:collapse;table-layout:fixed}th,td{padding:3px 0;vertical-align:top}td:last-child,th:last-child{text-align:right;width:27%}.line{break-inside:avoid}dl{margin:0}dl>div{display:flex;justify-content:space-between;gap:8px}dd{margin:0;text-align:right}.total{font-size:14px;font-weight:bold}img{display:block;width:36mm;height:36mm;max-width:100%;margin:5px auto;image-rendering:pixelated}.uuid{font-size:10px}thead{display:table-header-group}</style></head><body>'+
    '<header class="center"><h1>'+escape(ticket.emisor.nombre)+'</h1>'+p(ticket.emisor.razonSocial)+p(ticket.emisor.identificacion ? 'NIT: '+ticket.emisor.identificacion : '')+p(ticket.emisor.direccion)+
    '<h2>'+(fiscal?'FACTURA ELECTRÓNICA EN LÍNEA':'COMPROBANTE DE VENTA')+'</h2>'+(warning?'<p class="warning">'+escape(warning)+'</p>':'')+'</header>'+
    '<section>'+p('Venta interna: '+ticket.idVenta)+p('Emisión: '+date(ticket.fecha))+p('Vendedor: '+ticket.vendedor)+p('Receptor: '+ticket.receptor.nombre)+p(ticket.receptor.identificacion ? 'Identificación: '+ticket.receptor.identificacion : '')+p(ticket.receptor.direccion)+'</section>'+
    '<section><table><thead><tr><th style="text-align:left">Detalle</th><th>Total GTQ</th></tr></thead><tbody>'+ticket.lineas.map(l=>'<tr class="line"><td>'+escape(l.descripcion)+'<br>'+escape(l.cantidad)+' '+escape(l.unidad)+' × '+money(l.precioUnitario)+(l.descuento?' (descuento '+money(l.descuento)+')':'')+'</td><td>'+money(l.total)+'</td></tr>').join('')+'</tbody></table></section>'+
    '<section><dl><div><dt>Subtotal GTQ</dt><dd>'+money(ticket.subtotal)+'</dd></div><div><dt>Descuento GTQ</dt><dd>'+money(ticket.descuento)+'</dd></div><div class="total"><dt>TOTAL GTQ</dt><dd>'+money(ticket.total)+'</dd></div>'+(ticket.iva===null?'':'<div><dt>IVA incluido GTQ</dt><dd>'+money(ticket.iva)+'</dd></div>')+'</dl></section>'+
    (c?'<section>'+p('Serie: '+c.serie)+p('Número DTE: '+c.numero)+'<p class="uuid">Autorización: '+escape(c.uuid)+'</p>'+p('Certificación: '+date(c.fecha))+p('Certificador: '+c.nombreCertificador)+p('NIT certificador: '+c.nitCertificador)+ticket.frases.map(p).join('')+'<img src="'+escape(qrDataUrl)+'" alt="QR para consultar el DTE en SAT">'+p(ticket.ambiente==='PRUEBAS'?'PRUEBAS: no acredita validez fiscal.':'Consulta el DTE en SAT.')+'</section>':'')+
    ticket.avisos.filter(a=>a!==warning).map(p).join('')+'</body></html>'
}
