import test from 'node:test'
import assert from 'node:assert/strict'
import QRCode from 'qrcode'
import { loadTicketPos, parseTicketPos } from '../src/features/sales/api/ticketPosApi.ts'
import type { TicketPos } from '../src/features/sales/api/ticketPosApi.ts'
import { paperWidth, ticketPosHtml } from '../src/features/sales/utils/ticketPosHtml.ts'

const ticket: TicketPos = {idVenta:1,tipo:'COMPROBANTE_INTERNO',ambiente:null,fecha:'2026-11-21T12:00:00',vendedor:'VENDEDOR',emisor:{nombre:'NEGOCIO',razonSocial:null,identificacion:'5203333',direccion:'CIUDAD'},receptor:{nombre:'CLIENTE',razonSocial:null,identificacion:null,direccion:null},lineas:[{descripcion:'Producto',cantidad:1,unidad:'UND',precioUnitario:112,descuento:0,total:112}],subtotal:112,descuento:0,iva:12,total:112,certificacion:null,frases:[],avisos:[]}
const fiscal: TicketPos = {...ticket,tipo:'FACTURA_FEL',ambiente:'PRUEBAS',certificacion:{uuid:'15B0C102-4DD4-4ED6-8C5C-85B9515720B7',serie:'PRUEBAS',numero:'123',fecha:'2026-11-21T12:00:00-06:00',nombreCertificador:'CERTIFICADOR',nitCertificador:'1234',qrUrl:'https://portal.sat.gob.gt/portal/verificador-integrado/?tipo=autorizacion&documento=FEL&emisor=5203333&serie=PRUEBAS&numero=123'},frases:['Sujeto a pagos trimestrales ISR']}

test('Ticket: consulta autenticada sin enviar una orden a la impresora',async t=>{
 let calls=0
 t.mock.method(globalThis,'fetch',async(url:string,options:RequestInit)=>{calls++;assert.equal(url,'/api/comprobantes/ventas/1/ticket-pos');assert.equal(options.method,'GET');assert.equal((options.headers as Record<string,string>).Authorization,'Bearer prueba');return Response.json(ticket)})
 assert.equal((await loadTicketPos(1,'prueba')).total,112);assert.equal(calls,1)
})
test('Ticket: ID inválido no consulta al servidor',async()=>{await assert.rejects(loadTicketPos(0,'prueba'))})
test('Ticket: rechaza otra venta, campos faltantes, importes inválidos y certificación mezclada',()=>{
 for(const value of [{...ticket,idVenta:2},{...ticket,total:-1},{...ticket,total:NaN},{...ticket,lineas:[]},{...ticket,emisor:null},{...ticket,certificacion:fiscal.certificacion},{...fiscal,certificacion:null}])assert.throws(()=>parseTicketPos(value,1))
})
test('Ticket: QR solo a SAT y debe corresponder al emisor, serie y número',()=>{
 assert.equal(parseTicketPos(fiscal,1).tipo,'FACTURA_FEL')
 for(const url of ['https://otro.test/',fiscal.certificacion!.qrUrl.replace('5203333','9999'),fiscal.certificacion!.qrUrl.replace('numero=123','numero=124'),'javascript:alert(1)'])assert.throws(()=>parseTicketPos({...fiscal,certificacion:{...fiscal.certificacion,qrUrl:url}},1))
})
test('Ticket: tamaños 58 y 80 mm, sin impuestos agregados al total',()=>{
 for(const width of [58,80] as const){const html=ticketPosHtml(ticket,width);assert.ok(html.includes('width:'+width+'mm'));assert.ok(html.includes('112.00'));assert.ok(html.includes('IVA incluido'));assert.ok(html.includes('NO ES FACTURA FEL'));assert.ok(!html.includes('<img'))}
 assert.equal(paperWidth('58'),58);assert.equal(paperWidth('80'),80);assert.equal(paperWidth('incorrecto'),80)
})
test('Ticket: el HTML escapa texto del cliente y producto sin ejecutar código',()=>{
 const html=ticketPosHtml({...ticket,receptor:{...ticket.receptor,nombre:'<script>alert(1)</script>'},lineas:[{...ticket.lineas[0],descripcion:'<img src=x onerror="alert(1)"> & producto'}]},80)
 assert.ok(!html.includes('<script>'));assert.ok(!html.includes('<img src=x'));assert.ok(html.includes('&lt;script&gt;'));assert.ok(html.includes('&amp;'));assert.ok(html.includes('Content-Security-Policy'))
})
test('Ticket: DTE exige QR y conserva UUID, serie, número y aviso de pruebas',async()=>{
 assert.throws(()=>ticketPosHtml(fiscal,58))
 const qr=await QRCode.toDataURL(fiscal.certificacion!.qrUrl,{errorCorrectionLevel:'M',margin:4,width:384})
 assert.ok(qr.startsWith('data:image/png;base64,'))
 const html=ticketPosHtml(fiscal,58,qr)
 for(const value of [fiscal.certificacion!.uuid,'Serie: PRUEBAS','Número DTE: 123','SIN VALIDEZ FISCAL','Sujeto a pagos trimestrales ISR'])assert.ok(html.includes(value))
 assert.ok(html.includes('<img'))
})
test('Ticket: fecha guatemalteca no cambia al depender de la zona del navegador',()=>{
 const html=ticketPosHtml(ticket,80)
 assert.ok(html.includes('12:00'))
})
