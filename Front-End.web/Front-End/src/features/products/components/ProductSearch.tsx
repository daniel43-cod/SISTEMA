import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { searchProducts } from '../api/searchProducts'
import type { ProductSuggestion } from '../api/searchProducts'
import { ProductListPage } from '../pages/ProductListPage'

export function ProductSearch({ onResult }: { onResult: (active: boolean) => void }) {
  const { session, logout } = useAuth()
  const [mode, setMode] = useState<'nombre' | 'codigoBarra'>('nombre')
  const [text, setText] = useState('')
  const [suggestions, setSuggestions] = useState<ProductSuggestion[]>([])
  const [loading, setLoading] = useState(false)
  const [searched, setSearched] = useState(false)
  const [error, setError] = useState('')
  const [selected, setSelected] = useState<number | null>(null)
  const [blocked, setBlocked] = useState(false)
  const request = useRef<AbortController | null>(null)
  const cooldown = useRef<ReturnType<typeof setTimeout> | null>(null)
  const cache = useRef(new Map<string, { time: number; items: ProductSuggestion[] }>())
  const token = session?.user.role === 'ADMINISTRADOR' ? session.token : undefined
  useEffect(() => () => { request.current?.abort(); if (cooldown.current) clearTimeout(cooldown.current) }, [])
  useEffect(() => {
    if (!token || mode !== 'nombre' || text.trim().length < 3 || selected !== null || blocked) return
    const timer = setTimeout(() => { void query('nombre', text) }, 350)
    return () => { clearTimeout(timer); request.current?.abort() }
    // query uses the current token/text; this effect controls scheduling and cancellation.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [text, mode, token, selected, blocked])
  async function query(kind: 'nombre' | 'codigoBarra', value: string) {
    if (!token || blocked) return
    request.current?.abort()
    const controller = new AbortController()
    request.current = controller
    setLoading(true); setError(''); setSearched(false)
    try {
      const key = `${token}:${kind}:${value.trim()}`
      const cached = cache.current.get(key)
      const items = cached && Date.now() - cached.time < 30000 ? cached.items : await searchProducts(kind, value, token, controller.signal)
      if (controller.signal.aborted) return
      if (cache.current.size >= 40) cache.current.clear()
      cache.current.set(key, { time: Date.now(), items })
      if (kind === 'codigoBarra') {
        if (items.length) { setSelected(items[0].idProducto); onResult(true) }
        setSuggestions([])
      } else setSuggestions(items)
      setSearched(true)
    } catch (failure) {
      if (controller.signal.aborted) return
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else {
        setError(failure instanceof ApiError ? failure.message : 'No se pudo buscar el producto.')
        if (failure instanceof ApiError && failure.status === 429) {
          setBlocked(true)
          cooldown.current = setTimeout(() => setBlocked(false), (failure.retryAfterSeconds ?? 60) * 1000)
        }
      }
    } finally { if (!controller.signal.aborted) setLoading(false) }
  }
  function reset() {
    request.current?.abort(); setSelected(null); onResult(false); setSuggestions([]); setError(''); setSearched(false); setLoading(false)
  }
  function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (mode === 'codigoBarra') void query(mode, text)
  }
  if (!token) return null
  return <section className="product-search" aria-labelledby="product-search-title">
    <h2 id="product-search-title">Buscar producto</h2>
    <form className="product-search-controls" onSubmit={submit}>
      <label>Buscar por<select value={mode} onChange={event => { reset(); setText(''); setMode(event.target.value as typeof mode) }}><option value="nombre">Nombre</option><option value="codigoBarra">Código de barras</option></select></label>
      <label>{mode === 'nombre' ? 'Nombre del producto' : 'Código de barras'}<input type="search" value={text} maxLength={mode === 'nombre' ? 200 : 100}
        placeholder={mode === 'nombre' ? 'Escribe al menos 3 caracteres' : 'Código completo y Enter'}
        onChange={event => { reset(); setText(event.target.value) }}
        onKeyDown={event => { if (event.key === 'Escape') { reset(); setText('') } }} /></label>
      {mode === 'codigoBarra' && <button type="submit" disabled={loading || blocked || !text.trim()}>Buscar</button>}
      {text && <button type="button" onClick={() => { reset(); setText('') }}>Limpiar</button>}
    </form>
    {loading && <p role="status">Buscando…</p>}
    {error && <p role="alert" className="product-error">{error}</p>}
    {searched && !selected && !suggestions.length && <p role="status">No se encontraron productos.</p>}
    {mode === 'nombre' && suggestions.length > 0 && !selected && <ul className="product-search-suggestions" aria-label="Productos sugeridos">
      {suggestions.map(item => <li key={item.idProducto}><button type="button" onClick={() => { request.current?.abort(); setSelected(item.idProducto); setSuggestions([]); onResult(true) }}>{item.nombre}</button></li>)}
    </ul>}
    {selected !== null && <ProductListPage key={selected} productId={selected} />}
  </section>
}