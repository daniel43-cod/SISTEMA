import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { useAuth } from '../../auth'
import { useBrands } from '../../brands/hooks/useBrands'
import { ApiError } from '../../../shared/api/ApiError'
import { updateProduct } from '../api/updateProduct'
import { usePresentations } from '../../presentations/hooks/usePresentations'
import type { ProductDetail } from '../types/product'
import { ProductImagePicker } from './ProductImagePicker'

export function EditProductForm({ product, onSaved, onCancel, onPending, focusPresentations = false }: {
  focusPresentations?: boolean; product: ProductDetail; onSaved: () => void; onCancel: () => void; onPending: (value: boolean) => void
}) {
  const { session, logout } = useAuth()
  const presentationsPanel = useRef<HTMLElement>(null)
  useEffect(() => {
    if (focusPresentations) {
      presentationsPanel.current?.focus()
      presentationsPanel.current?.scrollIntoView({ block: 'nearest' })
    }
  }, [focusPresentations])
  const brands = useBrands()
  const catalog = usePresentations()
  const [rows, setRows] = useState(() => product.presentaciones.map(item => ({
    key: `existing-${item.idProductoPresentacion}`, id: item.idProductoPresentacion,
    presentation: String(item.idPresentacion), label: item.descripcion ?? 'Sin descripción',
    units: String(item.unidadesEquivalentes), price: String(item.precio), active: item.activa,
  })))
  const [presentationsChanged, setPresentationsChanged] = useState(false)
  const nextKey = useRef(0)
  function changeRow(key: string, patch: Partial<(typeof rows)[number]>) {
    setPresentationsChanged(true)
    setRows(previous => previous.map(row => row.key === key ? { ...row, ...patch } : row))
  }
  const [name, setName] = useState(product.nombre)
  const [code, setCode] = useState(product.codigoBarra ?? '')
  const [brand, setBrand] = useState(String(product.idMarca))
  const [minimum, setMinimum] = useState(String(product.stockMinimo))
  const [imageUrl, setImageUrl] = useState('')
  const [imageFile, setImageFile] = useState<File>()
  const [imageChanged, setImageChanged] = useState(false)
  const [pending, setPending] = useState(false)
  const [error, setError] = useState('')
  const request = useRef<AbortController | null>(null)
  useEffect(() => () => request.current?.abort(), [])
  async function save(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (request.current || !session || session.user.role !== 'ADMINISTRADOR') return
    const controller = new AbortController()
    request.current = controller
    setPending(true); onPending(true); setError('')
    try {
      if (!minimum.trim()) throw new ApiError('Ingresa la existencia mínima.', 400)
      await updateProduct(product.idProducto, { nombre: name, codigo_barra: code, idMarca: Number(brand), stock_minimo: Number(minimum),
        ...(imageChanged ? { urlImagen: imageUrl, imagen: imageFile, quitarImagen: !imageUrl.trim() && !imageFile } : {}),
        ...(presentationsChanged ? { presentaciones: rows.map(row => ({ ...(row.id ? { id_producto_presentacion: row.id } : {}),
          id_presentacion: Number(row.presentation), unidades_equivalentes: row.units.trim() ? Number(row.units) : 0,
          precio: row.price.trim() ? Number(row.price) : 0, estado: row.active })) } : {}) }, session.token, controller.signal)
      if (!controller.signal.aborted) onSaved()
    } catch (failure) {
      if (controller.signal.aborted) return
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else setError(failure instanceof ApiError ? failure.message : 'No se pudo actualizar el producto.')
    } finally {
      request.current = null
      if (!controller.signal.aborted) { setPending(false); onPending(false) }
    }
  }
  return <form className="product-form" onSubmit={save} aria-busy={pending}>
    <h3>Editar producto</h3>
    <fieldset className="product-fields" disabled={pending}>
      <label className="field">Nombre<input autoFocus={!focusPresentations} required maxLength={200} value={name} onChange={event => setName(event.target.value)} /></label>
      <label className="field">Código de barras<input required maxLength={100} value={code} onChange={event => setCode(event.target.value)} /></label>
      <label className="field">Marca<select required value={brand} disabled={brands.loading || Boolean(brands.error)} onChange={event => setBrand(event.target.value)}>
        {!brands.items.some(item => item.idMarca === product.idMarca) && <option value={product.idMarca} disabled>{product.marca} (no disponible)</option>}
        {brands.items.map(item => <option key={item.idMarca} value={item.idMarca}>{item.nombre}</option>)}
      </select></label>
      <label className="field">Existencia mínima<input required type="number" min="0" max="2147483647" step="1" value={minimum} onChange={event => setMinimum(event.target.value)} /></label>
      <section ref={presentationsPanel} tabIndex={-1} className="product-presentations" aria-labelledby="edit-presentations-title">
        <h3 id="edit-presentations-title">Editar presentaciones</h3>
        {!rows.length && <p>Este producto no tiene presentaciones. Puedes agregar una.</p>}
        {rows.map(row => <div className="product-row" key={row.key}>
          <label className="field">Presentación
            {row.id ? <input readOnly value={row.label} /> : <select required value={row.presentation} onChange={event => changeRow(row.key, { presentation: event.target.value })}>
              <option value="">Selecciona una presentación</option>
              {catalog.items.filter(item => !rows.some(other => other.key !== row.key && Number(other.presentation) === item.idPresentacion)).map(item =>
                <option key={item.idPresentacion} value={item.idPresentacion}>{item.descripcion}</option>)}
            </select>}
          </label>
          <label className="field">Unidades equivalentes<input required type="number" min="1" max="2147483647" step="1" value={row.units} onChange={event => changeRow(row.key, { units: event.target.value })} /></label>
          <label className="field">Precio de venta (Q)<input required type="number" min="0.01" step="0.01" value={row.price} onChange={event => changeRow(row.key, { price: event.target.value })} /></label>
          <label><input type="checkbox" checked={row.active} onChange={event => changeRow(row.key, { active: event.target.checked })} /> Activa</label>
          {!row.id && <button type="button" onClick={() => { setPresentationsChanged(true); setRows(previous => previous.filter(item => item.key !== row.key)) }}>Quitar presentación nueva</button>}
        </div>)}
        <button type="button" disabled={catalog.loading || Boolean(catalog.error) || rows.length >= 100}
          onClick={() => { setPresentationsChanged(true); setRows(previous => [...previous, { key: `new-${nextKey.current++}`, id: 0, presentation: '', label: '', units: '1', price: '', active: true }]); }}>Agregar presentación</button>
        {catalog.loading && <p role="status">Cargando presentaciones…</p>}
        {catalog.error && <><p role="alert" className="product-error">{catalog.error}</p><button type="button" onClick={catalog.reload}>Reintentar presentaciones</button></>}
      </section>
    </fieldset>
    <ProductImagePicker url={imageUrl} file={imageFile} currentImage={!imageChanged ? product.imagen ?? undefined : undefined}
      disabled={pending} onChange={(url, file) => { setImageUrl(url); setImageFile(file); setImageChanged(true) }} />
    {brands.loading && <p role="status">Cargando marcas…</p>}
    {brands.error && <><p role="alert" className="product-error">{brands.error}</p><button type="button" disabled={pending} onClick={brands.reload}>Reintentar marcas</button></>}
    {error && <p role="alert" className="product-error">{error}</p>}
    <div className="product-actions">
      <button type="submit" disabled={pending || brands.loading || Boolean(brands.error) || !brands.items.some(item => item.idMarca === Number(brand))}>{pending ? 'Guardando…' : 'Guardar cambios'}</button>
      <button type="button" disabled={pending} onClick={onCancel}>Cancelar</button>
    </div>
  </form>
}