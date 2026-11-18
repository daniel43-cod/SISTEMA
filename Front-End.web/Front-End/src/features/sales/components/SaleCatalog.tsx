import { useEffect, useId, useRef, useState } from 'react'
import { CatalogCategoriesList } from '../../public-catalog/components/CatalogCategoriesList'
import { CatalogProductCard } from '../../public-catalog/components/CatalogProductCard'
import { PublicBrandsList } from '../../public-catalog/components/PublicBrandsList'
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
  onClose: () => void
}
function SaleCatalogProducts({ brandId, options, lines, disabled, onAdd }: Omit<Props, 'onClose'> & { brandId: number }) {
  const { items, pagina, hayMas, loading, error, reload, setPagina } = usePublicProducts(brandId)
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
      : !items.length ? <p>No hay productos disponibles en esta página.</p>
      : <ul className="public-products-grid">
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
          const usedUnits = lines.filter(line => line.product.id_producto === product.idProducto)
            .reduce((sum, line) => sum + Number(line.quantity) * line.presentation.unidades_equivalentes, 0)
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
export function SaleCatalog({ onClose, ...props }: Props) {
  const categories = usePublicCategories()
  const [category, setCategory] = useState<PublicCategory | null>(null)
  const [brand, setBrand] = useState<PublicBrand | null>(null)

  const titleRef = useRef<HTMLHeadingElement>(null)
  const titleId = useId()
  useEffect(() => { titleRef.current?.focus() }, [category, brand])
  return <section className="public-catalog sale-catalog-page" aria-labelledby={titleId}>
    <header className="sale-catalog-toolbar">
      <button type="button" onClick={onClose}>← Volver a Nueva venta</button>
      <span>{props.lines.length} {props.lines.length === 1 ? 'presentación en la venta' : 'presentaciones en la venta'}</span>
    </header>
    <div className="sale-catalog-content">
      <div className="sale-catalog-heading">
        {category && <button type="button" aria-label={brand ? 'Volver a marcas' : 'Volver a categorías'}
          onClick={() => { if (brand) setBrand(null); else setCategory(null) }}>←</button>}
        <h1 ref={titleRef} tabIndex={-1} id={titleId}>{brand ? 'Productos de ' + brand.nombre : category ? 'Marcas de ' + category.nombre : 'Categorías'}</h1>
      </div>
      {brand ? <SaleCatalogProducts key={brand.idMarca} brandId={brand.idMarca} {...props} />
        : category ? <PublicBrandsList key={category.idCategoria} idCategoria={category.idCategoria} onSelect={setBrand} />
        : categories.loading ? <p role="status">Cargando categorías…</p>
        : categories.error ? <div role="alert"><p>{categories.error}</p><button type="button" onClick={categories.reload}>Reintentar</button></div>
        : !categories.items.length ? <p>No hay categorías disponibles.</p>
        : <CatalogCategoriesList items={categories.items} onSelect={setCategory} />}
    </div>
  </section>
}