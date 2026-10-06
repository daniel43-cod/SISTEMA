import { test } from 'node:test'
import assert from 'node:assert/strict'
import { createProduct } from '../src/features/products/api/productsApi.ts'
import { validateProduct } from '../src/features/products/schemas/productSchema.ts'
import { ApiError } from '../src/shared/api/ApiError.ts'

const valid = () => ({
  codigo_barra: '00123', nombre: 'Coca', idMarca: 1, stock_minimo: 0,
  presentaciones: [{ id_presentacion: 2, unidades_equivalentes: 6, precio: 25.50 }],
})
test('Producto valida cantidades enteras, precios y presentaciones repetidas', () => {
  assert.equal(validateProduct(valid()), undefined)
  assert.ok(validateProduct({ ...valid(), stock_minimo: -1 }))
  assert.ok(validateProduct({ ...valid(), presentaciones: [] }))
  assert.ok(validateProduct({ ...valid(), presentaciones: [valid().presentaciones[0], valid().presentaciones[0]] }))
  for (const change of [{ unidades_equivalentes: 1.5 }, { unidades_equivalentes: 0 }, { precio: NaN }, { precio: 1.001 }, { precio: 0 }])
    assert.ok(validateProduct({ ...valid(), presentaciones: [{ ...valid().presentaciones[0], ...change }] }))
})
test('Crear producto usa JSON autenticado sin enviar categoría, stock ni fecha', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Productos/crear')
    assert.equal(options.method, 'POST')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    assert.deepEqual(JSON.parse(options.body as string), valid())
    return Response.json({ id_producto: 3 }, { status: 201 })
  })
  assert.equal(await createProduct(valid(), 'token'), 3)
})
test('Imagen y presentaciones se envían con nombres indexados para ASP.NET', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Productos/crear/con-imagen')
    assert.ok(options.body instanceof FormData)
    assert.equal((options.headers as Record<string, string>)['Content-Type'], undefined)
    assert.equal(options.body.get('IdMarca'), '1')
    assert.equal(options.body.get('presentaciones[0].unidades_equivalentes'), '6')
    assert.equal(options.body.get('presentaciones[0].precio'), '25.5')
    assert.ok(options.body.get('Imagen') instanceof File)
    return Response.json({ id_producto: 3 })
  })
  await createProduct({ ...valid(), imagen: new File(['x'], 'a.jpg', { type: 'image/jpeg' }) }, 'token')
})
test('Duplicados, permisos y fallos de red no reintentan la creación', async t => {
  for (const status of [401, 403, 409]) {
    t.mock.method(globalThis, 'fetch', async () => new Response('secret', { status }))
    await assert.rejects(createProduct(valid(), 'token'), error =>
      error instanceof ApiError && error.status === status && !error.message.includes('secret'))
  }
  const mock = t.mock.method(globalThis, 'fetch', async () => { throw new TypeError('network') })
  await assert.rejects(createProduct(valid(), 'token'))
  assert.equal(mock.mock.callCount(), 1)
})

 test('Listado y detalle de productos usan GET autenticado y paginación', async t => {
  const { listProducts, getProductDetail } = await import('../src/features/products/api/productQueries.ts')
  const item = { idProducto: 1, nombre: 'Agua', marca: 'Marca A', categoria: 'Bebidas', marcaActiva: true,
    categoriaActiva: true, codigoBarra: null, stockUnidades: 25, stockMinimo: 0 }
  const mock = t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(options.method, 'GET')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    assert.equal(options.cache, 'no-store')
    if (url.includes('listar')) {
      assert.equal(url, '/api/Productos/listar?pagina=2&tamanoPagina=20')
      return Response.json({ pagina: 2, tamanoPagina: 20, total: 21, items: [item] })
    }
    assert.equal(url, '/api/Productos/1')
    return Response.json({ ...item, fechaCreacion: '2026-10-04', presentaciones: [] })
  })
  assert.equal((await listProducts(2, 'token')).items[0].marca, 'Marca A')
  assert.deepEqual((await getProductDetail(1, 'token')).presentaciones, [])
  assert.equal(mock.mock.callCount(), 2)
  await assert.rejects(listProducts(0, 'token'))
  await assert.rejects(getProductDetail(-1, 'token'))
  assert.equal(mock.mock.callCount(), 2)
})
test('Consultas rechazan contratos inválidos y conservan 401, 403, 404 y Retry-After', async t => {
  const { listProducts, getProductDetail } = await import('../src/features/products/api/productQueries.ts')
  t.mock.method(globalThis, 'fetch', async () => Response.json([]))
  await assert.rejects(listProducts(1, 'token'))
  await assert.rejects(getProductDetail(1, 'token'))
  for (const status of [401, 403, 404, 429]) {
    t.mock.method(globalThis, 'fetch', async () => new Response('secret', { status, headers: { 'Retry-After': '12' } }))
    await assert.rejects(getProductDetail(1, 'token'), error => error instanceof ApiError &&
      error.status === status && !error.message.includes('secret') && (status !== 429 || error.retryAfterSeconds === 12))
  }
})

test('Editar producto envía solo campos editables y maneja duplicados', async t => {
  const { updateProduct } = await import('../src/features/products/api/updateProduct.ts')
  const data = { nombre: ' Agua ', codigo_barra: ' 001 ', idMarca: 1, stock_minimo: 2 }
  const mock = t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Productos/1')
    assert.equal(options.method, 'PUT')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    assert.deepEqual(JSON.parse(options.body as string), { nombre: 'Agua', codigo_barra: '001', idMarca: 1, stock_minimo: 2 })
    return Response.json({ idProducto: 1 })
  })
  await updateProduct(1, data, 'token')
  await assert.rejects(updateProduct(1, { ...data, nombre: '' }, 'token'))
  assert.equal(mock.mock.callCount(), 1)
  t.mock.method(globalThis, 'fetch', async () => new Response('secret', { status: 409 }))
  await assert.rejects(updateProduct(1, data, 'token'), error => error instanceof ApiError && error.status === 409 && error.message.includes('nombre o código'))
})

test('Editar conserva IDs de presentaciones y valida duplicados antes de enviar', async t => {
  const { updateProduct } = await import('../src/features/products/api/updateProduct.ts')
  const row = { id_producto_presentacion: 9, id_presentacion: 2, unidades_equivalentes: 12, precio: 25.5, estado: false }
  const data = { nombre: 'Agua', codigo_barra: '001', idMarca: 1, stock_minimo: 2, presentaciones: [row] }
  const mock = t.mock.method(globalThis, 'fetch', async (_url: string, options: RequestInit) => {
    assert.deepEqual(JSON.parse(options.body as string).presentaciones, [row])
    return Response.json({ idProducto: 1 })
  })
  await updateProduct(1, data, 'token')
  await assert.rejects(updateProduct(1, { ...data, presentaciones: [row, row] }, 'token'))
  await assert.rejects(updateProduct(1, { ...data, presentaciones: [{ ...row, precio: 1.001 }] }, 'token'))
  assert.equal(mock.mock.callCount(), 1)
})

test('Búsqueda administrativa envía nombre o código y devuelve sugerencias sin presentaciones', async t => {
  const { searchProducts } = await import('../src/features/products/api/searchProducts.ts')
  const mock = t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    assert.ok(url === '/api/Productos/buscar-administracion?nombre=Coca' || url === '/api/Productos/buscar-administracion?codigoBarra=001')
    return Response.json([{ idProducto: 1, nombre: 'Coca Cola' }])
  })
  assert.equal((await searchProducts('nombre', ' Coca ', 'token'))[0].nombre, 'Coca Cola')
  assert.equal((await searchProducts('codigoBarra', '001', 'token'))[0].idProducto, 1)
  await assert.rejects(searchProducts('nombre', 'co', 'token'))
  assert.equal(mock.mock.callCount(), 2)
  t.mock.method(globalThis, 'fetch', async () => Response.json([{ idProducto: 0, nombre: 'Inválido' }]))
  await assert.rejects(searchProducts('nombre', 'Coca', 'token'))
})

test('Carga nombres una vez y filtra en memoria sin nuevas solicitudes', async t => {
  const { loadProductNames, filterProductNames } = await import('../src/features/products/api/searchProducts.ts')
  const mock = t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Productos/nombres')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    return Response.json([{ idProducto: 1, nombre: 'Coca Cola 2.5', stock: 5 }, { idProducto: 2, nombre: 'Coca Cola 3 litros' }, { idProducto: 3, nombre: 'Café' }])
  })
  const names = await loadProductNames('token')
  assert.deepEqual(Object.keys(names[0]), ['idProducto', 'nombre'])
  assert.equal(filterProductNames(names, 'co').length, 0)
  assert.equal(filterProductNames(names, 'coc').length, 2)
  assert.equal(filterProductNames(names, 'coca c').length, 2)
  assert.equal(filterProductNames(names, 'CAFE')[0].idProducto, 3)
  assert.equal(mock.mock.callCount(), 1)
  assert.equal(filterProductNames(Array.from({ length: 15 }, (_, i) => ({ idProducto: i + 1, nombre: 'Coca ' + i })), 'coca').length, 10)
})
