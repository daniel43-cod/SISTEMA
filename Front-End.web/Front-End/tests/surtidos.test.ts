import { test } from 'node:test'
import assert from 'node:assert/strict'
import { createSurtido } from '../src/features/products/api/surtidosApi.ts'
import { listSurtidos } from '../src/features/products/api/surtidosApi.ts'
import { updateSurtido, getSurtido } from '../src/features/products/api/surtidosApi.ts'
import { ApiError } from '../src/shared/api/ApiError.ts'

const data = { nombre: 'Cola surtida', idMarca: 1, idsProductos: [2, 3],
  presentaciones: [{ idPresentacion: 4, unidadesEquivalentes: 6, precio: 16.2 }] }

test('Editar surtido usa PUT, conserva selección y envía intención de quitar imagen', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/surtidos/8')
    assert.equal(options.method, 'PUT')
    assert.deepEqual(JSON.parse(options.body as string), { ...data, quitarImagen: true, estado: false })
    return Response.json({ idSurtido: 8 })
  })
  assert.equal(await updateSurtido(8, { ...data, quitarImagen: true, estado: false }, 'token'), 8)
})

test('Editar imagen de surtido usa PUT multipart con estado y presentaciones', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/surtidos/8/con-imagen')
    assert.equal(options.method, 'PUT')
    assert.ok(options.body instanceof FormData)
    assert.ok(options.body.get('Imagen') instanceof File)
    assert.equal(options.body.get('QuitarImagen'), 'false')
    assert.equal(options.body.get('Estado'), 'true')
    assert.equal(options.body.get('Presentaciones[0].idPresentacion'), '4')
    return Response.json({ idSurtido: 8 })
  })
  await updateSurtido(8, { ...data, imagen: new File(['img'], 'foto.webp'), quitarImagen: false, estado: true }, 'token')
})

test('Detalle incluye identificadores para precargar marca, productos y presentaciones al editar', async t => {
  const detail = { idSurtido: 8, idMarca: 1, nombre: 'Cola', marca: 'Marca', estado: true, imagen: null,
    productos: [{ idProducto: 2, nombre: 'Limón', costoPromedio: 2.5 }],
    presentaciones: [{ idProductoPresentacion: 7, idPresentacion: 4, presentacion: 'Bandeja', unidadesEquivalentes: 6, precio: 20, precioMinimoActual: 15 }] }
  t.mock.method(globalThis, 'fetch', async () => Response.json(detail))
  assert.deepEqual(await getSurtido(8, 'token'), detail)
})

test('Tarjetas de surtidos filtran por marca e incluyen imagen y precios', async t => {
  const card = { idSurtido: 8, nombre: 'Cola surtida', idMarca: 1, marca: 'Cola', estado: true, productos: 2,
    imagen: '/uploads/productos/imagen.webp', presentaciones: [{ nombre: 'Bandeja', unidadesEquivalentes: 6, precio: 16.2 }] }
  t.mock.method(globalThis, 'fetch', async (url: string) => {
    assert.equal(new URL(url, 'https://local.test').searchParams.get('idMarca'), '1')
    return Response.json({ total: 1, pagina: 1, tamano: 20, elementos: [card] })
  })
  assert.deepEqual((await listSurtidos('', 1, 'token', undefined, 1)).elementos, [card])
})

test('Imagen de surtido usa multipart con productos y presentaciones indexados', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/surtidos/con-imagen')
    assert.ok(options.body instanceof FormData)
    assert.ok(options.body.get('Imagen') instanceof File)
    assert.equal(options.body.get('IdsProductos[1]'), '3')
    assert.equal(options.body.get('Presentaciones[0].unidadesEquivalentes'), '6')
    return Response.json({ idSurtido: 8 })
  })
  assert.equal(await createSurtido({ ...data, imagen: new File(['test'], 'foto.webp') }, 'token'), 8)
})

test('Surtido envía selección y presentaciones autenticadas sin calcular costos en el cliente', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/surtidos')
    assert.equal(options.method, 'POST')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    assert.deepEqual(JSON.parse(options.body as string), data)
    return Response.json({ idSurtido: 8 }, { status: 201 })
  })
  assert.equal(await createSurtido(data, 'token'), 8)
})

test('Surtido muestra validación de compatibilidad del servidor sin reintentar el guardado', async t => {
  const mock = t.mock.method(globalThis, 'fetch', async () => Response.json({ mensaje: 'El precio mínimo es Q16.20.' }, { status: 400 }))
  await assert.rejects(createSurtido(data, 'token'), e => e instanceof ApiError && e.status === 400 && e.message === 'El precio mínimo es Q16.20.')
  assert.equal(mock.mock.callCount(), 1)
})

test('Surtido no confirma éxito ante una respuesta sin identificador válido', async t => {
  t.mock.method(globalThis, 'fetch', async () => Response.json({ idSurtido: 0 }))
  await assert.rejects(createSurtido(data, 'token'), /No se pudo confirmar el registro/)
})
