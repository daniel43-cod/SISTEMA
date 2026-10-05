import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { useAuth } from '../../auth'
import { useBrands } from '../../brands/hooks/useBrands'
import { ApiError } from '../../../shared/api/ApiError'
import { updateProduct } from '../api/updateProduct'
import type { ProductDetail } from '../types/product'

export function EditProductForm({ product, onSaved, onCancel, onPending }: {
  product: ProductDetail; onSaved: () => void; onCancel: () => void; onPending: (value: boolean) => void
}) {
  const { session, logout } = useAuth()
  const brands = useBrands()
  const [name, setName] = useState(product.nombre)
  const [code, setCode] = useState(product.codigoBarra ?? '')
  const [brand, setBrand] = useState(String(product.idMarca))
  const [minimum, setMinimum] = useState(String(product.stockMinimo))
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
      await updateProduct(product.idProducto, { nombre: name, codigo_barra: code, idMarca: Number(brand), stock_minimo: Number(minimum) }, session.token, controller.signal)
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
      <label className="field">Nombre<input autoFocus required maxLength={200} value={name} onChange={event => setName(event.target.value)} /></label>
      <label className="field">Código de barras<input required maxLength={100} value={code} onChange={event => setCode(event.target.value)} /></label>
      <label className="field">Marca<select required value={brand} disabled={brands.loading || Boolean(brands.error)} onChange={event => setBrand(event.target.value)}>
        {!brands.items.some(item => item.idMarca === product.idMarca) && <option value={product.idMarca} disabled>{product.marca} (no disponible)</option>}
        {brands.items.map(item => <option key={item.idMarca} value={item.idMarca}>{item.nombre}</option>)}
      </select></label>
      <label className="field">Existencia mínima<input required type="number" min="0" max="2147483647" step="1" value={minimum} onChange={event => setMinimum(event.target.value)} /></label>
    </fieldset>
    {brands.loading && <p role="status">Cargando marcas…</p>}
    {brands.error && <><p role="alert" className="product-error">{brands.error}</p><button type="button" disabled={pending} onClick={brands.reload}>Reintentar marcas</button></>}
    {error && <p role="alert" className="product-error">{error}</p>}
    <div className="product-actions">
      <button type="submit" disabled={pending || brands.loading || Boolean(brands.error) || !brands.items.some(item => item.idMarca === Number(brand))}>{pending ? 'Guardando…' : 'Guardar cambios'}</button>
      <button type="button" disabled={pending} onClick={onCancel}>Cancelar</button>
    </div>
  </form>
}