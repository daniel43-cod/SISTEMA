import { test } from 'node:test'
import assert from 'node:assert/strict'
import { listBrands } from '../src/features/brands/api/brandsApi.ts'
import { ApiError } from '../src/shared/api/ApiError.ts'

test('Listado de marcas usa GET autenticado y admite lista vacía', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Marcas')
    assert.equal(options.method, 'GET')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    return Response.json([])
  })
  assert.deepEqual(await listBrands('token'), [])
})
test('Listado valida marcas, categorías e identificadores duplicados', async t => {
  const row = { idMarca: 1, nombre: 'Marca', idCategoria: 2, estado: true }
  t.mock.method(globalThis, 'fetch', async () => Response.json([row]))
  assert.deepEqual(await listBrands('token'), [row])
  for (const data of [{}, [row, row], [{ ...row, idCategoria: 0 }], [{ ...row, estado: false }]]) {
    t.mock.method(globalThis, 'fetch', async () => Response.json(data))
    await assert.rejects(listBrands('token'), ApiError)
  }
})
test('Listado conserva errores de sesión y permisos sin exponer detalles', async t => {
  for (const status of [401, 403, 500]) {
    t.mock.method(globalThis, 'fetch', async () => new Response('secret', { status }))
    await assert.rejects(listBrands('token'), error =>
      error instanceof ApiError && error.status === status && !error.message.includes('secret'))
  }
})

test('Editar marca envía PUT con nombre y categoría, sin estado ni ID en el cuerpo', async t => {
  const { updateBrand } = await import('../src/features/brands/api/brandsApi.ts')
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Marcas/3')
    assert.equal(options.method, 'PUT')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    assert.deepEqual(JSON.parse(options.body as string), { nombre: 'Marca', idCategoria: 2 })
    return Response.json({ idMarca: 3, nombre: 'Marca', idCategoria: 2, estado: true })
  })
  assert.equal((await updateBrand(3, { nombre: ' Marca ', idCategoria: 2 }, 'token')).idMarca, 3)
})
test('Editar rechaza ID inválido y conserva errores de permisos, duplicado y registro inexistente', async t => {
  const { updateBrand } = await import('../src/features/brands/api/brandsApi.ts')
  const mock = t.mock.method(globalThis, 'fetch', async () => new Response(null, { status: 404 }))
  await assert.rejects(updateBrand(0, { nombre: 'Marca', idCategoria: 2 }, 'token'))
  assert.equal(mock.mock.callCount(), 0)
  for (const status of [401, 403, 404, 409]) {
    t.mock.method(globalThis, 'fetch', async () => new Response('secret', { status }))
    await assert.rejects(updateBrand(3, { nombre: 'Marca', idCategoria: 2 }, 'token'),
      error => error instanceof ApiError && error.status === status && !error.message.includes('secret'))
  }
})

test('Crear marca con imagen envía multipart autenticado al nuevo endpoint', async t => {
  const { createBrand } = await import('../src/features/brands/api/brandsApi.ts')
  const file = new File(['png'], 'marca.png', { type: 'image/png' })
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Marcas/con-imagen')
    assert.equal(options.method, 'POST')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    assert.equal((options.headers as Record<string, string>)['Content-Type'], undefined)
    const body = options.body as FormData
    assert.equal(body.get('Nombre'), 'Marca')
    assert.equal(body.get('IdCategoria'), '2')
    assert.equal((body.get('Imagen') as File).name, 'marca.png')
    return Response.json({ idMarca: 1, nombre: 'Marca', idCategoria: 2, estado: true, urlImagen: '/uploads/marcas/a.webp' })
  })
  assert.equal((await createBrand({ nombre: ' Marca ', idCategoria: 2, imagen: file }, 'token')).urlImagen, '/uploads/marcas/a.webp')
})

test('Crear marca rechaza archivos inválidos sin realizar peticiones', async t => {
  const { createBrand } = await import('../src/features/brands/api/brandsApi.ts')
  const mock = t.mock.method(globalThis, 'fetch', async () => Response.json({}))
  for (const file of [new File(['svg'], 'a.svg', { type: 'image/svg+xml' }),
    new File([], 'a.png', { type: 'image/png' }),
    new File([new Uint8Array(5 * 1024 * 1024 + 1)], 'a.png', { type: 'image/png' })]) {
    await assert.rejects(createBrand({ nombre: 'Marca', idCategoria: 2, imagen: file }, 'token'), ApiError)
  }
  assert.equal(mock.mock.callCount(), 0)
})

test('Crear sin imagen conserva JSON y el listado conserva la ruta de imagen', async t => {
  const { createBrand } = await import('../src/features/brands/api/brandsApi.ts')
  const row = { idMarca: 1, nombre: 'Marca', idCategoria: 2, estado: true, urlImagen: '/uploads/marcas/a.webp' }
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Marcas')
    if (options.method === 'POST') {
      assert.deepEqual(JSON.parse(options.body as string), { nombre: 'Marca', idCategoria: 2 })
      return Response.json(row)
    }
    return Response.json([row])
  })
  await createBrand({ nombre: 'Marca', idCategoria: 2 }, 'token')
  assert.deepEqual(await listBrands('token'), [row])
})

test('Crear marca con enlace usa JSON y rechaza enlace junto con archivo', async t => {
  const { createBrand } = await import('../src/features/brands/api/brandsApi.ts')
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Marcas')
    assert.deepEqual(JSON.parse(options.body as string), { nombre: 'Marca', idCategoria: 2, urlImagen: 'https://example.com/marca.png' })
    return Response.json({ idMarca: 1, nombre: 'Marca', idCategoria: 2, estado: true, urlImagen: 'https://example.com/marca.png' })
  })
  await createBrand({ nombre: 'Marca', idCategoria: 2, urlImagen: ' https://example.com/marca.png ' }, 'token')
  const mock = t.mock.method(globalThis, 'fetch', async () => { throw new Error('No debe consultar') })
  for (const urlImagen of ['http://example.com/a.png', 'https://user:pass@example.com/a.png', 'invalido'])
    await assert.rejects(createBrand({ nombre: 'Marca', idCategoria: 2, urlImagen }, 'token'))
  await assert.rejects(createBrand({ nombre: 'Marca', idCategoria: 2, urlImagen: 'https://example.com/a.png',
    imagen: new File(['png'], 'a.png', { type: 'image/png' }) }, 'token'))
  assert.equal(mock.mock.callCount(), 0)
})

test('Editar marca admite archivo, enlace, quitar y conservar imagen', async t => {
  const { updateBrand } = await import('../src/features/brands/api/brandsApi.ts')
  const row = { idMarca: 3, nombre: 'Marca', idCategoria: 2, estado: true }
  for (const changes of [{}, { urlImagen: 'https://example.com/a.png' }, { quitarImagen: true }]) {
    t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
      assert.equal(url, '/api/Marcas/3')
      assert.equal(options.method, 'PUT')
      assert.deepEqual(JSON.parse(options.body as string), { nombre: 'Marca', idCategoria: 2, ...changes })
      return Response.json(row)
    })
    await updateBrand(3, { nombre: 'Marca', idCategoria: 2, ...changes }, 'token')
  }
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Marcas/3/con-imagen')
    assert.equal(options.method, 'PUT')
    assert.equal((options.headers as Record<string, string>)['Content-Type'], undefined)
    assert.equal(((options.body as FormData).get('Imagen') as File).name, 'a.png')
    return Response.json(row)
  })
  await updateBrand(3, { nombre: 'Marca', idCategoria: 2, imagen: new File(['png'], 'a.png', { type: 'image/png' }) }, 'token')
  const mock = t.mock.method(globalThis, 'fetch', async () => { throw new Error('No debe consultar') })
  await assert.rejects(updateBrand(3, { nombre: 'Marca', idCategoria: 2, quitarImagen: true, urlImagen: 'https://example.com/a.png' }, 'token'))
  assert.equal(mock.mock.callCount(), 0)
})

test('Administración lista marcas inactivas y cambia estado con PATCH autenticado', async t => {
  const { changeBrandState } = await import('../src/features/brands/api/brandsApi.ts')
  const row = { idMarca: 1, nombre: 'Marca', idCategoria: 2, estado: false, urlImagen: '/uploads/marcas/a.webp' }
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    if (options.method === 'GET') { assert.equal(url, '/api/Marcas/administracion'); return Response.json([row]) }
    assert.equal(url, '/api/Marcas/1/estado')
    assert.equal(options.method, 'PATCH')
    assert.deepEqual(JSON.parse(options.body as string), { estado: false })
    return Response.json(row)
  })
  assert.deepEqual(await listBrands('token', undefined, true), [row])
  assert.deepEqual(await changeBrandState(1, false, 'token'), row)
})

test('Cambio de estado de marca valida ID, respuesta y permisos', async t => {
  const { changeBrandState } = await import('../src/features/brands/api/brandsApi.ts')
  const mock = t.mock.method(globalThis, 'fetch', async () => Response.json({ idMarca: 2, nombre: 'Otra', idCategoria: 1, estado: false }))
  await assert.rejects(changeBrandState(0, false, 'token'))
  assert.equal(mock.mock.callCount(), 0)
  await assert.rejects(changeBrandState(1, false, 'token'))
  t.mock.method(globalThis, 'fetch', async () => Response.json({ idMarca: 1, nombre: 'Marca', idCategoria: 2, estado: true }))
  await assert.rejects(changeBrandState(1, false, 'token'))
  for (const status of [401, 403, 404, 500]) {
    t.mock.method(globalThis, 'fetch', async () => new Response('secret', { status }))
    await assert.rejects(changeBrandState(1, false, 'token'), error => error instanceof ApiError && error.status === status && !error.message.includes('secret'))
  }
})
