import { useId, useState } from 'react'
import type { FormEvent } from 'react'
import { useBrands } from '../../brands'
import { usePresentations } from '../../presentations'
import { Input } from '../../../shared/ui/Input'
import { Button } from '../../../shared/ui/Button'
import { ProductImagePicker } from './ProductImagePicker'
import { useCreateProduct } from '../hooks/useCreateProduct'

type Row = { key: number; presentation: string; units: string; price: string }
export function ProductForm({ onSaved }: { onSaved: () => void }) {
  const brands = useBrands()
  const presentations = usePresentations()
  const { submit, pending, error } = useCreateProduct()
  const [code, setCode] = useState('')
  const [name, setName] = useState('')
  const [minimum, setMinimum] = useState('0')
  const [brand, setBrand] = useState('')
  const [url, setUrl] = useState('')
  const [file, setFile] = useState<File>()
  const [rows, setRows] = useState<Row[]>([{ key: 0, presentation: '', units: '1', price: '' }])
  const [nextKey, setNextKey] = useState(1)
  const [validation, setValidation] = useState('')
  const brandId = useId()
  const loading = brands.loading || presentations.loading
  const listError = brands.error || presentations.error
  const unavailable = !brands.items.length || !presentations.items.length
  function update(key: number, field: 'presentation' | 'units' | 'price', value: string) {
    setRows(current => current.map(row => row.key === key ? { ...row, [field]: value } : row))
  }
  async function save(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setValidation('')
    if (loading || listError || unavailable) return
    if (!brands.items.some(item => item.idMarca === Number(brand)) ||
        rows.some(row => !presentations.items.some(item => item.idPresentacion === Number(row.presentation)))) {
      setValidation('Selecciona una marca y presentaciones disponibles.'); return
    }
    if (!/^\d+$/.test(minimum) || rows.some(row => !/^\d+$/.test(row.units) || !/^\d+(\.\d{1,2})?$/.test(row.price))) {
      setValidation('Usa enteros para las cantidades y un precio con máximo dos decimales.'); return
    }
    if (await submit({
      codigo_barra: code, nombre: name, idMarca: Number(brand), stock_minimo: Number(minimum),
      urlImagen: url, imagen: file,
      presentaciones: rows.map(row => ({
        id_presentacion: Number(row.presentation), unidades_equivalentes: Number(row.units), precio: Number(row.price),
      })),
    })) onSaved()
  }
  return <form className="product-form" onSubmit={save} aria-busy={pending}>
    <fieldset disabled={pending} className="product-fields">
      <Input label="Código de barras" value={code} required maxLength={100} onChange={event => setCode(event.target.value)} />
      <Input label="Nombre del producto" value={name} required maxLength={200} onChange={event => setName(event.target.value)} />
      <Input label="Stock mínimo (unidades)" type="number" inputMode="numeric" min="0" max="2147483647" step="1"
        value={minimum} required onChange={event => setMinimum(event.target.value)} />
      <div className="field">
        <label htmlFor={brandId}>Marca</label>
        <select id={brandId} className="input" required value={brand} disabled={loading || !!listError}
          onChange={event => setBrand(event.target.value)}>
          <option value="">Selecciona una marca</option>
          {brands.items.map(item => <option key={item.idMarca} value={item.idMarca}>
            {item.nombre}{item.nombreCategoria ? ' — ' + item.nombreCategoria : ''}
          </option>)}
        </select>
      </div>
    </fieldset>
    {loading && <p role="status">Cargando marcas y presentaciones…</p>}
    {listError && <div role="alert"><p>{listError}</p><Button disabled={pending || loading}
      onClick={() => { brands.reload(); presentations.reload() }}>Reintentar</Button></div>}
    {!loading && !listError && unavailable && <p role="status">Primero registra marcas y presentaciones en sus apartados.</p>}
    <ProductImagePicker url={url} file={file} disabled={pending} onChange={(value, image) => { setUrl(value); setFile(image) }} />
    <fieldset className="product-presentations" disabled={pending || loading || !!listError}>
      <legend>Presentaciones de venta</legend>
      <p>Indica cuántas unidades contiene cada presentación. Ejemplo: unidad = 1, bandeja = 6.</p>
      {rows.map((row, index) => <fieldset key={row.key} className="product-row">
        <legend>Presentación {index + 1}</legend>
        <label className="field">Presentación
          <select className="input" value={row.presentation} required onChange={event => update(row.key, 'presentation', event.target.value)}>
            <option value="">Selecciona una presentación</option>
            {presentations.items.map(item => <option key={item.idPresentacion} value={item.idPresentacion}
              disabled={rows.some(other => other.key !== row.key && Number(other.presentation) === item.idPresentacion)}>
              {item.descripcion}
            </option>)}
          </select>
        </label>
        <Input label="Unidades equivalentes" type="number" inputMode="numeric" required min="1" max="2147483647" step="1"
          value={row.units} onChange={event => update(row.key, 'units', event.target.value)} />
        <Input label="Precio de venta" type="number" inputMode="decimal" required min="0.01" max="9999999999999.99" step="0.01"
          value={row.price} onChange={event => update(row.key, 'price', event.target.value)} />
        <button type="button" disabled={rows.length === 1} onClick={() => setRows(current => current.filter(item => item.key !== row.key))}
          aria-label={'Quitar presentación ' + (index + 1)}>Quitar</button>
      </fieldset>)}
      <Button disabled={rows.length >= Math.min(100, presentations.items.length)} onClick={() => {
        setRows(current => [...current, { key: nextKey, presentation: '', units: '1', price: '' }]); setNextKey(value => value + 1)
      }}>Agregar presentación</Button>
    </fieldset>
    {(validation || error) && <p role="alert" className="product-error">{validation || error}</p>}
    <Button type="submit" disabled={pending || loading || !!listError || unavailable}>{pending ? 'Guardando…' : 'Guardar producto'}</Button>
  </form>
}
