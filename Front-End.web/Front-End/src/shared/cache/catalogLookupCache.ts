// Memoria de esta pestaña. Nunca se guarda en localStorage ni sessionStorage.
const TTL_MS = 5 * 60 * 1000
let generation = 0
let entry: { token: string; expiresAt: number; promise: Promise<unknown> } | undefined

export function clearCatalogLookupCache() {
  generation++
  entry = undefined
}

export function readCatalogLookupCache<T>(token: string, load: () => Promise<T>): Promise<T> {
  if (entry?.token === token && entry.expiresAt > Date.now())
    return entry.promise as Promise<T>
  clearCatalogLookupCache()
  const version = generation
  const promise = Promise.resolve().then(load).then(value => {
    if (version !== generation) throw new DOMException('La consulta fue invalidada.', 'AbortError')
    if (entry?.promise === promise) entry.expiresAt = Date.now() + TTL_MS
    return value
  }).catch(error => {
    if (entry?.promise === promise) entry = undefined
    throw error
  })
  entry = { token, expiresAt: Infinity, promise }
  return promise
}
