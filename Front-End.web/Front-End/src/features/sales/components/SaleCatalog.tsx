import { searchCatalogProducts, catalogSearchCards } from '../utils/catalogSearch'
import { useEffect, useId, useRef, useState } from 'react'
import { CatalogCategoriesList } from '../../public-catalog/components/CatalogCategoriesList'
import { CatalogProductCard } from '../../public-catalog/components/CatalogProductCard'
import { CatalogSurtidoCard } from '../../public-catalog/components/CatalogSurtidoCard'
import type { SurtidoCatalogSelection } from '../../public-catalog/types/product'
import { PublicBrandsList } from '../../public-catalog/components/PublicBrandsList'
import { usePublicBrands } from '../../public-catalog/hooks/usePublicBrands'
import { usePublicCategories } from '../../public-catalog/hooks/usePublicCategories'
import { usePublicProducts } from '../../public-catalog/hooks/usePublicProducts'
import type { PublicCategory } from '../../public-catalog/types/category'
import type { PublicBrand } from '../../public-catalog/types/brand'
import type { SaleOption } from '../utils/saleSearch'
import type { SaleLine } from '../utils/saleValidation'
import '../../public-catalog/pages/PublicCategoriesPage.css'
import './SaleCatalog.css'

type Props = {
  options: SaleOption[]; lines: SaleLine[]; disabled: boolean
  onAdd: (productId: number, selections: { presentationId: number; quantity: string }[]) => string | null
  onAddSurtidos: (idSurtido: number, selections: SurtidoCatalogSelection[]) => string | null
  onClose: () => void
}
function SaleCatalogProducts({ brandId, ...props }: Omit<Props, 'onClose'> & { brandId: number }) {
  const page = usePublicProducts(brandId)
  return <SaleCatalogCards {...props} {...page} />
}
function SaleCatalogSearchResults({ query, ...props }: Omit<Props, 'onClose'> & { query: string }) {
  const [pagina, setPagina] = useState(1)
  const results = searchCatalogProducts(props.options, query)
  const cards = catalogSearchCards(results.slice((pagina - 1) * 24, pagina * 24))
  return <>
    <p role="status" className="sale-catalog-search-status">{results.length} {results.length === 1 ? 'resultado' : 'resultados'} en todo el catálogo</p>
    <SaleCatalogCards {...props} {...cards} pagina={pagina} setPagina={setPagina} hayMas={pagina * 24 < results.length} loading={false} error="" reload={() => {}} />
  </>
}
function SaleCatalogCards({ items, surtidos, pagina, hayMas, loading, error, reload, setPagina, options, lines, disabled, onAdd, onAddSurtidos }:
  Omit<Props, 'onClose'> & ReturnType<typeof usePublicProducts>) {
  const [quantities, setQuantities] = useState<Record<number, string>>({})
  const [message, setMessage] = useState('')
  const [addError, setAddError] = useState('')
  function add(productId: number, presentationIds: number[]) {
    setMessage(''); setAddError('')
    const selections = presentationIds
      .filter(id => !lines.some(line => line.presentation.id_producto_presentacion === id))
      .map(id => ({ presentationId: id, quantity: quantities[id] ?? '0' }))
      .filter(selection => selection.quantity !== '0')
    const failure = onAdd(productId, selections)
    if (failure) setAddError(failure)
    else {
      setMessage('Producto agregado. Podés seguir seleccionando o volver a Nueva venta.')
      setQuantities(current => {
        const next = { ...current }
        for (const id of presentationIds) delete next[id]
        return next
      })
    }
  }
  return <>
    {message && <p role="status" className="sale-catalog-success">{message}</p>}
    {addError && <p role="alert" className="sale-catalog-error">{addError}</p>}
    {loading ? <p role="status">Cargando productos…</p> : error ? <div role="alert"><p>{error}</p><button type="button" onClick={reload}>Reintentar</button></div>
      : !items.length && !surtidos.length ? <p>No hay productos disponibles en esta página.</p>
      : <ul className="public-products-grid">
        {surtidos.map(s => <CatalogSurtidoCard key={`surtido-${s.idSurtido}`} product={{ ...s, presentaciones: s.presentaciones.map(p => {
          const option = options.find(o => o.product.id_surtido === s.idSurtido && o.presentation.id_producto_presentacion === p.idProductoPresentacion)
          return { ...p, precio: option?.presentation.precio ?? p.precio, disponible: p.disponible && !!option && !option.product.bloqueo }
        }) }} disabled={disabled} lockedIds={lines.map(l => l.presentation.id_producto_presentacion)} actionLabel="Agregar a la venta"
          onAdd={selections => onAddSurtidos(s.idSurtido, selections)} />)}
        {items.map(product => <CatalogProductCard key={product.idProducto} product={{
          ...product,
          presentaciones: product.presentaciones.map(p => {
            const option = options.find(o => o.product.id_producto === product.idProducto && o.presentation.id_producto_presentacion === p.idProductoPresentacion)
            return { ...p, precio: option?.presentation.precio ?? p.precio,
              disponible: p.disponible && !!option && option.product.stock >= option.presentation.unidades_equivalentes }
          })
        }} footer={<div className="sale-catalog-card-footer">
          <button type="button" className="sale-catalog-add" disabled={disabled ||
            product.presentaciones.every(p => lines.some(line => line.presentation.id_producto_presentacion === p.idProductoPresentacion))}
            onClick={() => add(product.idProducto, product.presentaciones.map(p => p.idProductoPresentacion))}>Agregar a la venta</button>
        </div>} renderActions={presentation => {
          const id = presentation.idProductoPresentacion
          const option = options.find(o => o.product.id_producto === product.idProducto && o.presentation.id_producto_presentacion === id)
          const alreadyAdded = lines.some(line => line.presentation.id_producto_presentacion === id)
          const usedUnits = lines.reduce((sum, line) => sum + (line.product.id_surtido
            ? line.componentes?.find(c => c.id_producto === product.idProducto)?.cantidad ?? 0
            : line.product.id_producto === product.idProducto ? Number(line.quantity) * line.presentation.unidades_equivalentes : 0), 0)
          const max = option ? Math.max(0, Math.floor((option.product.stock - usedUnits) / option.presentation.unidades_equivalentes)) : 0
          const raw = quantities[id] ?? '0'
          const quantity = /^\d+$/.test(raw) ? Number(raw) : 0
          const unavailable = disabled || !presentation.disponible || !option || max < 1
          const label = product.nombre + ', ' + presentation.nombre
          return <div className="sale-catalog-presentation-actions">
            <div className="public-quantity" role="group" aria-label={'Cantidad a agregar de ' + label}>
              <button type="button" aria-label={'Reducir cantidad de ' + label} disabled={unavailable || alreadyAdded || quantity <= 0}
                onClick={() => setQuantities(q => ({ ...q, [id]: String(Math.max(0, quantity - 1)) }))}>−</button>
              <input aria-label={'Cantidad de ' + label} inputMode="numeric" value={raw} disabled={unavailable || alreadyAdded}
                onChange={e => setQuantities(q => ({ ...q, [id]: e.target.value }))} />
              <button type="button" aria-label={'Aumentar cantidad de ' + label} disabled={unavailable || alreadyAdded || quantity >= max}
                onClick={() => setQuantities(q => ({ ...q, [id]: String(quantity + 1) }))}>+</button>
            </div>
            {alreadyAdded && <p className="sale-catalog-added">Ya está en la venta. Editala desde Nueva venta.</p>}
            {!option && <small>No disponible para esta venta.</small>}
          </div>
        }} />)}
      </ul>}
    <nav className="public-pagination" aria-label="Páginas de productos">
      <button type="button" disabled={disabled || loading || pagina === 1} onClick={() => { setPagina(pagina - 1); setAddError(''); setMessage('') }}>Anterior</button>
      <span>Página {pagina}</span>
      <button type="button" disabled={disabled || loading || !!error || !hayMas} onClick={() => { setPagina(pagina + 1); setAddError(''); setMessage('') }}>Siguiente</button>
    </nav>
  </>
}
function BrandSwitcher({ categoryId, selectedId, disabled, onSelect }: {
  categoryId: number; selectedId: number; disabled: boolean; onSelect: (brand: PublicBrand) => void
}) {
  const { items, loading, error, reload } = usePublicBrands(categoryId)
  const nav = useRef<HTMLElement>(null)
  useEffect(() => {
    nav.current?.querySelector<HTMLElement>('[aria-pressed="true"]')?.scrollIntoView({ block: 'nearest', inline: 'center' })
  }, [selectedId, items])
  return <nav className="sale-catalog-brand-switcher" aria-label="Marcas de esta categoría" ref={nav}>
    {loading ? <p role="status">Cargando marcas…</p> : error ? <div role="alert"><p>{error}</p><button type="button" onClick={reload}>Reintentar marcas</button></div> :
      <div className="sale-catalog-brand-options">{items.map(item => <button key={item.idMarca} type="button" disabled={disabled} aria-pressed={selectedId === item.idMarca}
        onClick={() => { if (selectedId !== item.idMarca) onSelect(item) }}>{item.nombre}</button>)}</div>}
  </nav>
}
export function SaleCatalog({ onClose, ...props }: Props) {
  const categories = usePublicCategories()
  const [search, setSearch] = useState('')
  const query = search.trim()
  const toolbarRef = useRef<HTMLElement>(null)
  useEffect(() => {

    const toolbar = toolbarRef.current
    const catalog = toolbar?.closest<HTMLElement>('.sale-catalog-page')
    if (!toolbar || !catalog) return
    const measure = () => catalog.style.setProperty('--sale-catalog-toolbar-height', `${toolbar.getBoundingClientRect().height}px`)
    measure()
    const observer = new ResizeObserver(measure)
    observer.observe(toolbar)
    return () => { observer.disconnect(); catalog.style.removeProperty('--sale-catalog-toolbar-height') }
  }, [])
  const [category, setCategory] = useState<PublicCategory | null>(null)
  const [brand, setBrand] = useState<PublicBrand | null>(null)

  const [categoryMenuOpen, setCategoryMenuOpen] = useState(false)
  const categoryMenu = useRef<HTMLDivElement>(null)
  const categoryTrigger = useRef<HTMLButtonElement>(null)
  const categoryMenuId = useId()
  useEffect(() => {
    if (!categoryMenuOpen) return
    const closeOutside = (event: PointerEvent) => {
      if (event.target instanceof Node && !categoryMenu.current?.contains(event.target)) setCategoryMenuOpen(false)
    }
    document.addEventListener('pointerdown', closeOutside)
    return () => document.removeEventListener('pointerdown', closeOutside)
  }, [categoryMenuOpen])
  const titleRef = useRef<HTMLHeadingElement>(null)
  const titleId = useId()
  useEffect(() => { titleRef.current?.focus() }, [category, brand])
  return <section className="public-catalog sale-catalog-page" aria-labelledby={titleId}>
    <header className="sale-catalog-toolbar" ref={toolbarRef}>
      {!category && <strong className="sale-catalog-categories-label">CATEGORÍAS</strong>}
      {category && <div className="sale-catalog-category-menu" ref={categoryMenu} onKeyDown={event => {
        if (event.key === 'Escape') { event.preventDefault(); setCategoryMenuOpen(false); categoryTrigger.current?.focus() }
      }}>
        <button type="button" ref={categoryTrigger} className="sale-catalog-category-trigger" aria-expanded={categoryMenuOpen} aria-controls={categoryMenuId}
          disabled={props.disabled || categories.loading || !!categories.error} onClick={() => setCategoryMenuOpen(open => !open)}>
          <span>{category.nombre}</span><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true"><path d="m6 9 6 6 6-6" /></svg>
        </button>
        {categoryMenuOpen && <ul id={categoryMenuId} className="sale-catalog-category-list" aria-label="Categorías">
          {categories.items.map(item => <li key={item.idCategoria}><button type="button" aria-pressed={category.idCategoria === item.idCategoria} onClick={() => {
            setBrand(null); setCategory(item); setCategoryMenuOpen(false); setSearch('')
          }}>{item.nombre}</button></li>)}
        </ul>}
      </div>}
      <button type="button" className="sale-catalog-back" onClick={onClose}>Ver venta</button>
    </header>
    <div className="sale-catalog-content">
      <div className="sale-catalog-search"><input type="search" aria-label="Buscar producto en todo el catálogo" placeholder="Buscar producto" maxLength={200}
        value={search} disabled={props.disabled} onChange={event => setSearch(event.target.value)} />
        {query && <button type="button" disabled={props.disabled} aria-label="Limpiar búsqueda" title="Limpiar búsqueda" onClick={() => setSearch('')}>×</button>}
      </div>
      <div className={`sale-catalog-heading${!category ? ' is-categories' : !brand ? ' is-brands' : ' is-products'}`}>
        <h1 ref={titleRef} tabIndex={-1} id={titleId}>{brand ? 'Productos de ' + brand.nombre : category ? 'Marcas de ' + category.nombre : 'Categorías'}</h1>
      </div>
      {!query && category && brand && <BrandSwitcher key={category.idCategoria} categoryId={category.idCategoria} selectedId={brand.idMarca} disabled={props.disabled} onSelect={setBrand} />}
      {query ? <SaleCatalogSearchResults key={query} query={query} {...props} /> : brand ? <SaleCatalogProducts key={brand.idMarca} brandId={brand.idMarca} {...props} />
        : category ? <PublicBrandsList key={category.idCategoria} idCategoria={category.idCategoria} onSelect={setBrand} />
        : categories.loading ? <p role="status">Cargando categorías…</p>
        : categories.error ? <div role="alert"><p>{categories.error}</p><button type="button" onClick={categories.reload}>Reintentar</button></div>
        : !categories.items.length ? <p>No hay categorías disponibles.</p>
        : <CatalogCategoriesList items={categories.items} onSelect={setCategory} />}
    </div>
  </section>
}
