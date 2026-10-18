import json
from pathlib import Path
root = Path(r'C:\Users\coy81\Desktop\SISTEMA\SISTEMA\Front-End.web\Front-End')
files = []
def add(path, content): files.append({'path': path, 'content': content})
p = 'src/features/inventory/api/inventoryApi.ts'
s = (root / p).read_text(encoding='utf-8')
s = s.replace('signal: AbortSignal): Promise<InventoryPageData>', 'signal: AbortSignal, filters: { idCategoria?: number; idMarca?: number } = {}): Promise<InventoryPageData>')
s = s.replace("  const result = await requestJson", "  if (filters.idCategoria) params.set('idCategoria', String(filters.idCategoria))\n  if (filters.idMarca) params.set('idMarca', String(filters.idMarca))\n  const result = await requestJson")
add(p,s)
add('src/features/inventory/components/InventoryLookup.tsx', '''import { useState } from 'react'

export interface InventoryOption { id: number; nombre: string }
export function InventoryLookup({ label, items, selected, loading, error, onSelect }: {
  label: string; items: InventoryOption[]; selected?: number; loading: boolean; error: string
  onSelect: (id?: number) => void
}) {
  const [query, setQuery] = useState('')
  const [open, setOpen] = useState(false)
  const normalize = (value: string) => value.normalize('NFD').replace(/[\\u0300-\\u036f]/g, '').toLocaleLowerCase('es').trim()
  const matches = items.filter(item => normalize(item.nombre).includes(normalize(query)))
  const id = 'inventory-' + label.toLowerCase().normalize('NFD').replace(/[\\u0300-\\u036f]/g, '')
  return <div className="inventory-search inventory-lookup" onBlur={event => {
    if (!event.currentTarget.contains(event.relatedTarget)) setOpen(false)
  }}>
    <label htmlFor={id}>{label}</label>
    <input id={id} type="search" autoComplete="off" maxLength={100}
      value={query} placeholder={'Buscar ' + label.toLowerCase() + '…'}
      aria-expanded={open} aria-controls={id + '-options'}
      onFocus={() => setOpen(true)} onKeyDown={event => {
        if (event.key === 'Escape') setOpen(false)
        if (event.key === 'Enter' && matches.length === 1) {
          event.preventDefault(); setQuery(matches[0].nombre); onSelect(matches[0].id); setOpen(false)
        }
      }} onChange={event => {
        setQuery(event.target.value); setOpen(true)
        if (selected !== undefined) onSelect(undefined)
      }} />
    {open && <div id={id + '-options'} className="inventory-suggestions">
      {loading ? <p role="status">Cargando…</p> : error ? <p role="alert">{error}</p>
        : matches.length ? <ul aria-label={label + ': coincidencias'}>{matches.map(item => <li key={item.id}>
          <button type="button" onClick={() => { setQuery(item.nombre); onSelect(item.id); setOpen(false) }}>{item.nombre}</button>
        </li>)}</ul> : <p role="status">Sin coincidencias.</p>}
    </div>}
  </div>
}
''')
p = 'src/features/inventory/pages/InventoryPage.tsx'
s = (root / p).read_text(encoding='utf-8')
s = "import { InventoryLookup } from '../components/InventoryLookup'\nimport { loadCatalogLookups } from '../../catalog-lookups/api/catalogLookupsApi'\nimport type { CatalogLookups } from '../../catalog-lookups/api/catalogLookupsApi'\nimport { clearCatalogLookupCache } from '../../../shared/cache/catalogLookupCache'\n" + s
s = s.replace("  const [data, setData]", "  const [categoryId, setCategoryId] = useState<number>()\n  const [brandId, setBrandId] = useState<number>()\n  const [lookups, setLookups] = useState<CatalogLookups>({ categories: [], brands: [] })\n  const [lookupLoading, setLookupLoading] = useState(true)\n  const [lookupError, setLookupError] = useState('')\n  const [data, setData]")
at = "  useEffect(() => {\n    if (!token || !allowed) return"
effect = '''  useEffect(() => {
    if (!token || !allowed) return
    let active = true
    async function load() {
      setLookupLoading(true); setLookupError('')
      try {
        const result = await loadCatalogLookups(token!)
        if (active) setLookups(result)
      } catch (failure) {
        if (!active) return
        if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
        else setLookupError(failure instanceof Error ? failure.message : 'No se pudieron cargar categorías y marcas.')
      } finally { if (active) setLookupLoading(false) }
    }
    void load()
    return () => { active = false }
  }, [token, allowed, revision, logout])

'''
s = s.replace(at, effect + at, 1)
s = s.replace('listInventory(page, text, token, controller.signal)', 'listInventory(page, text, token, controller.signal, { idCategoria: categoryId, idMarca: brandId })')
s = s.replace('[token, allowed, page, text, revision, logout]', '[token, allowed, page, text, categoryId, brandId, revision, logout]')
s = s.replace('className="sr-only">Buscar producto por nombre', '>Producto')
s = s.replace('onKeyDown={event => { if (event.key === \'Escape\') setSuggestionsOpen(false) }}', "onKeyDown={event => {\n            if (event.key === 'Escape') setSuggestionsOpen(false)\n            if (event.key === 'Enter') { setText(query.trim()); setPage(1); setSuggestionsOpen(false) }\n          }}")
s = s.replace('      <RefreshButton loading={loading || names.loading}', '''      <InventoryLookup label="Categoría" items={lookups.categories.map(item => ({ id: item.idCategoria, nombre: item.nombre }))}
        selected={categoryId} loading={lookupLoading} error={lookupError}
        onSelect={id => { setCategoryId(id); setPage(1) }} />
      <InventoryLookup label="Marca" items={lookups.brands.map(item => ({ id: item.idMarca, nombre: item.nombre }))}
        selected={brandId} loading={lookupLoading} error={lookupError}
        onSelect={id => { setBrandId(id); setPage(1) }} />
      <RefreshButton loading={loading || names.loading || lookupLoading}''')
s = s.replace('onClick={() => setRevision(value => value + 1)} />', 'onClick={() => { clearCatalogLookupCache(); setRevision(value => value + 1) }} />')
add(p,s)
p = 'src/features/inventory/pages/InventoryPage.css'
s = (root / p).read_text(encoding='utf-8')
s += '''
.inventory-toolbar .inventory-search { flex: 1 1 200px; }
.inventory-search > label { display: block; margin-bottom: 8px; }
.inventory-suggestions li button:focus-visible { outline: 2px solid var(--color-primary); outline-offset: -3px; }
@media (max-width: 600px) { .inventory-toolbar .inventory-search { flex-basis: 100%; } }
'''
add(p,s)
Path('.artifacts/inventory-files.json').write_text(json.dumps(files, ensure_ascii=False), encoding='utf-8')
