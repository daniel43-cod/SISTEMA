import { test } from 'node:test'
import assert from 'node:assert/strict'
import { createCategory } from '../src/features/categories/api/categoriesApi.ts'
import { parseCategory, validateCategoryName } from '../src/features/categories/schemas/categorySchema.ts'
import { ApiError } from '../src/shared/api/ApiError.ts'

test('Categoría valida nombre y contrato de respuesta', () => {
  assert.ok(validateCategoryName('   '))
  assert.ok(validateCategoryName('x'.repeat(101)))
  assert.equal(validateCategoryName('x'.repeat(100)), undefined)
  assert.deepEqual(parseCategory({ idCategoria: 1, nombre: 'Bebidas', estado: true }),
    { idCategoria: 1, nombre: 'Bebidas', estado: true })
  for (const value of [null, {}, { idCategoria: '1', nombre: 'Bebidas', estado: true }])
    assert.throws(() => parseCategory(value))
})

test('Crear categoría envía nombre limpio y token al endpoint correcto', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Categoria')
    assert.equal(options.method, 'POST')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer test-token')
    assert.deepEqual(JSON.parse(options.body as string), { nombre: 'Bebidas' })
    return Response.json({ idCategoria: 2, nombre: 'Bebidas', estado: true }, { status: 201 })
  })
  assert.equal((await createCategory({ nombre: ' Bebidas ' }, 'test-token')).idCategoria, 2)
})

test('Categoría conserva permisos y sesión; traduce duplicados sin exponer detalles', async t => {
  for (const status of [401, 403, 409]) {
    t.mock.method(globalThis, 'fetch', async () => new Response('secret', { status }))
    await assert.rejects(createCategory({ nombre: 'Bebidas' }, 'token'), error =>
      error instanceof ApiError && error.status === status && !error.message.includes('secret') &&
      (status !== 409 || error.message.includes('Ya existe')))
    t.mock.restoreAll()
  }
})

test('Categoría no envía nombres vacíos ni reintenta operaciones de resultado incierto', async t => {
  const mocked = t.mock.method(globalThis, 'fetch', async () => { throw new TypeError('network') })
  await assert.rejects(createCategory({ nombre: ' ' }, 'token'))
  assert.equal(mocked.mock.callCount(), 0)
  await assert.rejects(createCategory({ nombre: 'Bebidas' }, 'token'), error =>
    error instanceof ApiError && error.message.includes('verifica si se guardó'))
  assert.equal(mocked.mock.callCount(), 1)
})
test('Imagen local viaja como multipart con token y sin Content-Type manual', async t => {
  const file = new File(['image-bytes'], 'foto.jpg', { type: 'image/jpeg' })
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Categoria/con-imagen')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    assert.equal((options.headers as Record<string, string>)['Content-Type'], undefined)
    assert.ok(options.body instanceof FormData)
    assert.equal(options.body.get('Nombre'), 'Bebidas')
    assert.equal((options.body.get('Imagen') as File).name, 'foto.jpg')
    return Response.json({ idCategoria: 1, nombre: 'Bebidas', estado: true })
  })
  await createCategory({ nombre: ' Bebidas ', imagen: file }, 'token')
})

test('URL externa viaja como JSON; rechaza enlace inseguro, archivo excesivo y ambos orígenes', async t => {
  const mock = t.mock.method(globalThis, 'fetch', async (_url: string, options: RequestInit) => {
    assert.deepEqual(JSON.parse(options.body as string), { nombre: 'Bebidas', urlImagen: 'https://example.com/a.jpg' })
    return Response.json({ idCategoria: 1, nombre: 'Bebidas', estado: true })
  })
  await createCategory({ nombre: 'Bebidas', urlImagen: 'https://example.com/a.jpg' }, 'token')
  await assert.rejects(createCategory({ nombre: 'Bebidas', urlImagen: 'javascript:alert(1)' }, 'token'))
  await assert.rejects(createCategory({ nombre: 'Bebidas', imagen: new File([new Uint8Array(5 * 1024 * 1024 + 1)], 'a.jpg', { type: 'image/jpeg' }) }, 'token'))
  await assert.rejects(createCategory({ nombre: 'Bebidas', urlImagen: 'https://example.com/a.jpg', imagen: new File(['x'], 'a.jpg') }, 'token'))
  assert.equal(mock.mock.callCount(), 1)
})

test('Listar categorías usa GET autenticado y admite una lista vacía', async t => {
  const { listCategories } = await import('../src/features/categories/api/categoriesApi.ts')
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Categoria')
    assert.equal(options.method, 'GET')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    return Response.json([])
  })
  assert.deepEqual(await listCategories('token'), [])
})

test('Actualizar conserva imagen por defecto y permite quitarla explícitamente', async t => {
  const { updateCategory } = await import('../src/features/categories/api/categoriesApi.ts')
  let quitar = false
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Categoria/7')
    assert.equal(options.method, 'PUT')
    assert.deepEqual(JSON.parse(options.body as string), { nombre: 'Bebidas', quitarImagen: quitar })
    return Response.json({ idCategoria: 7, nombre: 'Bebidas', estado: true })
  })
  await updateCategory(7, { nombre: 'Bebidas' }, 'token')
  quitar = true
  await updateCategory(7, { nombre: 'Bebidas', quitarImagen: true }, 'token')
})

test('Actualizar imagen usa multipart y rechaza ID de respuesta diferente', async t => {
  const { updateCategory } = await import('../src/features/categories/api/categoriesApi.ts')
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Categoria/7/con-imagen')
    assert.equal(options.method, 'PUT')
    assert.ok(options.body instanceof FormData)
    assert.equal(options.body.get('Nombre'), 'Bebidas')
    assert.ok(options.body.get('Imagen') instanceof File)
    return Response.json({ idCategoria: 8, nombre: 'Bebidas', estado: true })
  })
  await assert.rejects(updateCategory(7, { nombre: 'Bebidas', imagen: new File(['x'], 'a.png', { type: 'image/png' }) }, 'token'))
})

test('Administración lista categorías inactivas y cambia estado con PATCH autenticado', async t => {
  const { listCategories, changeCategoryState } = await import('../src/features/categories/api/categoriesApi.ts')
  const row = { idCategoria: 1, nombre: 'Bebidas', estado: false }
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer token')
    if (options.method === 'GET') { assert.equal(url, '/api/Categoria/administracion'); return Response.json([row]) }
    assert.equal(url, '/api/Categoria/1/estado')
    assert.equal(options.method, 'PATCH')
    assert.deepEqual(JSON.parse(options.body as string), { estado: false })
    return Response.json(row)
  })
  assert.deepEqual(await listCategories('token', undefined, true), [row])
  assert.deepEqual(await changeCategoryState(1, false, 'token'), row)
})

test('Cambio de estado de categoría valida ID, respuesta y permisos', async t => {
  const { changeCategoryState } = await import('../src/features/categories/api/categoriesApi.ts')
  const mock = t.mock.method(globalThis, 'fetch', async () => Response.json({ idCategoria: 2, nombre: 'Otra', estado: false }))
  await assert.rejects(changeCategoryState(0, false, 'token'))
  assert.equal(mock.mock.callCount(), 0)
  await assert.rejects(changeCategoryState(1, false, 'token'))
  t.mock.method(globalThis, 'fetch', async () => Response.json({ idCategoria: 1, nombre: 'Bebidas', estado: true }))
  await assert.rejects(changeCategoryState(1, false, 'token'))
  for (const status of [401, 403, 404, 500]) {
    t.mock.method(globalThis, 'fetch', async () => new Response('secret', { status }))
    await assert.rejects(changeCategoryState(1, false, 'token'), error => error instanceof ApiError && error.status === status && !error.message.includes('secret'))
  }
})
