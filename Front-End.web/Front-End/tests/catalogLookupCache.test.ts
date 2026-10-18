import { test } from 'node:test'
import assert from 'node:assert/strict'
import { clearCatalogLookupCache, readCatalogLookupCache } from '../src/shared/cache/catalogLookupCache.ts'

test('comparte consultas simultáneas y resultados de la sesión', async () => {
  clearCatalogLookupCache()
  let calls = 0
  const load = async () => { calls++; return ['Bebidas'] }
  const first = readCatalogLookupCache('a', load)
  const second = readCatalogLookupCache('a', load)
  assert.equal(first, second)
  assert.deepEqual(await first, ['Bebidas'])
  await readCatalogLookupCache('a', load)
  assert.equal(calls, 1)
})

test('cambio de sesión e invalidación descartan respuestas pendientes', async () => {
  clearCatalogLookupCache()
  let finish!: (value: string[]) => void
  const old = readCatalogLookupCache('a', () => new Promise<string[]>(resolve => { finish = resolve }))
  await Promise.resolve()
  const rejected = assert.rejects(old, { name: 'AbortError' })
  assert.deepEqual(await readCatalogLookupCache('b', async () => ['Nueva sesión']), ['Nueva sesión'])
  finish(['Anterior'])
  await rejected
  clearCatalogLookupCache()
  assert.deepEqual(await readCatalogLookupCache('b', async () => ['Actualizado']), ['Actualizado'])
})

test('los fallos no quedan cacheados', async () => {
  clearCatalogLookupCache()
  await assert.rejects(readCatalogLookupCache('a', async () => { throw new Error('Sin conexión') }))
  assert.equal(await readCatalogLookupCache('a', async () => 'Reintento'), 'Reintento')
})

test('renueva el listado después de cinco minutos', async () => {
  clearCatalogLookupCache()
  const realNow = Date.now
  let now = 1000
  Date.now = () => now
  try {
    assert.equal(await readCatalogLookupCache('a', async () => 'Antes'), 'Antes')
    now += 300001
    assert.equal(await readCatalogLookupCache('a', async () => 'Después'), 'Después')
  } finally { Date.now = realNow; clearCatalogLookupCache() }
})
