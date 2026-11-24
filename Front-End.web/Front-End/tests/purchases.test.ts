import test from 'node:test'
import assert from 'node:assert/strict'
import { getPurchaseSummary } from '../src/features/purchases/api/purchasesApi.ts'

const summary = { totalRegistros: 1, cantidadPendientes: 1, cantidadPagadas: 1, totalComprado: 300, totalPorPagar: 200, totalPagado: 100, hayMas: false,
  items: [{ id_compra: 1, nombre_usuario: 'Admin', nombre_proveedor: 'Proveedor', descripcion_estado_compra: 'Pendiente', fecha_ingreso: '2026-11-21T12:00:00-06:00', total_compra: 300, saldo_pendiente: 200 }] }

for (const status of ['', 'pendientes', 'pagadas']) test('Compras: resumen y filtro backend ' + (status || 'todas'), async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    const parsed = new URL(url, 'https://local.test')
    assert.equal(parsed.pathname, '/api/Compra/resumen')
    assert.equal(parsed.searchParams.get('pagina'), '2')
    assert.equal(parsed.searchParams.get('tamanoPagina'), '20')
    assert.equal(parsed.searchParams.get('estadoPago'), status || null)
    assert.equal((options.headers as Record<string,string>).Authorization, 'Bearer test')
    return Response.json(summary)
  })
  const result = await getPurchaseSummary('test', 2, '', status)
  assert.equal(result.totalComprado, 300)
  assert.equal(result.totalPorPagar, 200)
})

test('Compras: filtros inválidos no hacen solicitudes', async t => {
  let calls=0
  t.mock.method(globalThis, 'fetch', async () => { calls++; return Response.json(summary) })
  for(const [page,status] of [[0,''],[100001,''],[1,'otro']] as const)
    await assert.rejects(getPurchaseSummary('test', page, '', status))
  assert.equal(calls,0)
})
test('Compras: sin caja activa admite resumen vacío', async t => {
  t.mock.method(globalThis, 'fetch', async () => Response.json({totalRegistros:0,cantidadPendientes:0,cantidadPagadas:0,totalComprado:0,totalPorPagar:0,totalPagado:0,hayMas:false,items:[]}))
  assert.equal((await getPurchaseSummary('test',1,'','')).totalPorPagar,0)
})
test('Compras: rechaza totales inválidos del servidor', async t => {
  t.mock.method(globalThis, 'fetch', async () => Response.json({...summary,totalPorPagar:-1}))
  await assert.rejects(getPurchaseSummary('test',1,'',''))
})

import { buildPurchase, purchaseSupplier } from '../src/features/purchases/utils/purchaseValidation.ts'
const purchaseLines = [{ idProducto: 1, nombre: 'Producto', cantidad: '2', total: '10' }]
test('Compras: distribuidora seleccionada envía solo su ID', () => {
  const payload = buildPurchase('12', '', '0', purchaseLines, 'Distribuidora seleccionada')
  assert.equal(payload.id_proveedor, 12)
  assert.equal(payload.nombre_proveedor, undefined)
})
test('Compras: nueva distribuidora envía nombre recortado sin ID', () => {
  const payload = buildPurchase('', '', '0', purchaseLines, '  Nueva distribuidora  ')
  assert.equal(payload.nombre_proveedor, 'Nueva distribuidora')
  assert.equal(payload.id_proveedor, undefined)
  assert.equal(payload.detalle_compra[0].precio, 10)
})
test('Compras: distribuidora vacía inválida o nombre demasiado largo se rechaza', () => {
  for (const name of ['', '   ', 'x'.repeat(101), 'Nombre\nOculto'])
    assert.throws(() => purchaseSupplier('', name))
  for (const id of ['-1', '0', 'abc', '1.5', '2147483648'])
    assert.throws(() => purchaseSupplier(id, 'Nombre'))
})
test('Compras: nombre numérico es nombre de distribuidora y admite límite de longitud', () => {
  assert.deepEqual(purchaseSupplier('', '123'), { nombre_proveedor: '123' })
  assert.equal(purchaseSupplier('', 'x'.repeat(100)).nombre_proveedor?.length, 100)
})
