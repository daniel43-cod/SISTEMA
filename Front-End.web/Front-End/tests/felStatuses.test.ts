import test from 'node:test'
import assert from 'node:assert/strict'
import { loadFelStatuses } from '../src/features/sales/api/felApi.ts'
import { felStatusView } from '../src/features/sales/utils/felStatusView.ts'

test('FEL lista: una consulta autenticada por lote',async t=>{
 let calls=0
 t.mock.method(globalThis,'fetch',async(url:string,options:RequestInit)=>{calls++;assert.equal(url,'/api/Fel/ventas/estados');assert.equal(options.method,'POST');assert.deepEqual(JSON.parse(options.body as string),[1,2]);assert.equal((options.headers as Record<string,string>).Authorization,'Bearer test');return Response.json([{id_venta:1,estado:'CERTIFICADA',ambiente:'PRUEBAS'},{id_venta:2,estado:'SIN_CERTIFICAR',ambiente:'PRUEBAS'}])})
 const states=await loadFelStatuses([1,2],'test');assert.equal(states.length,2);assert.equal(calls,1)
})
test('FEL lista: rechaza respuestas incompletas, ventas ajenas, duplicadas o estados desconocidos',async t=>{
 const first={id_venta:1,estado:'CERTIFICADA',ambiente:'PRUEBAS'}
 for(const result of [[],[first],[first,first],[first,{...first,id_venta:3}],[first,{...first,id_venta:2,estado:'OTRO'}],[first,{...first,id_venta:2,ambiente:'OTRO'}]]){
  t.mock.method(globalThis,'fetch',async()=>Response.json(result));await assert.rejects(loadFelStatuses([1,2],'test'));t.mock.restoreAll()
 }
})
test('FEL lista: entradas inválidas no consultan la API y lote vacío no envía peticiones',async t=>{
 let calls=0;t.mock.method(globalThis,'fetch',async()=>{calls++;return Response.json([])})
 for(const ids of [[0],[1,1],Array.from({length:101},(_,i)=>i+1)])await assert.rejects(loadFelStatuses(ids,'test'))
 assert.deepEqual(await loadFelStatuses([],'test'),[]);assert.equal(calls,0)
})
test('FEL lista: diferencia certificación, pagos, pruebas y estado desconocido',()=>{
 for(const estado of ['SIN_CERTIFICAR','PENDIENTE','CERTIFICADA','RECHAZADA','EN_PROCESO','POR_CONFIRMAR'] as const){const view=felStatusView({id_venta:1,estado,ambiente:'PRODUCCION'});assert.ok(view.label);assert.ok(view.tone)}
 assert.equal(felStatusView(undefined).label,'FEL no disponible');assert.equal(felStatusView(undefined,true).label,'Consultando FEL…')
 assert.equal(felStatusView({id_venta:1,estado:'CERTIFICADA',ambiente:'PRUEBAS'}).label,'Certificada · prueba')
 assert.equal(felStatusView({id_venta:1,estado:'POR_CONFIRMAR',ambiente:'PRUEBAS'}).label,'Por confirmar')
})
