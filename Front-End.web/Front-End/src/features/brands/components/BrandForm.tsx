import { BrandImagePicker } from './BrandImagePicker'
import { useId, useState } from 'react'
import type { FormEvent } from 'react'
import { Input } from '../../../shared/ui/Input'
import { Button } from '../../../shared/ui/Button'
import { useCategories } from '../../categories'
import { useSaveBrand } from '../hooks/useSaveBrand'
import type { BrandResponse } from '../types/brand'

export function BrandForm({ onSaved, brand }: { onSaved: () => void; brand?: BrandResponse }) {
  const [name, setName] = useState(brand?.nombre ?? '')
  const [category, setCategory] = useState(brand ? String(brand.idCategoria) : '')
  const [image, setImage] = useState<File>()
  const [imageUrl, setImageUrl] = useState('')
  const [removeImage, setRemoveImage] = useState(false)
  const id = useId()
  const { items, loading, error: listError, reload } = useCategories()
  const { submit, pending, error } = useSaveBrand(brand?.idMarca)
  const active = items.filter(item => item.estado)
  async function save(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!active.some(item => item.idCategoria === Number(category))) return
    if (await submit({ nombre: name, idCategoria: Number(category), imagen: image, urlImagen: imageUrl, ...(brand && removeImage && !image && !imageUrl.trim() ? { quitarImagen: true } : {}) })) onSaved()
  }
  return <form className="brand-form" onSubmit={save} aria-busy={pending}>
    <Input label="Nombre de la marca" value={name} onChange={event => setName(event.target.value)}
      required maxLength={100} disabled={pending} placeholder="Ejemplo: Frito-Lay" />
    <div className="field">
      <label htmlFor={id}>Categoría</label>
      <select id={id} className="input" value={category} required disabled={pending || loading || !!listError}
        onChange={event => setCategory(event.target.value)}>
        <option value="">{loading ? 'Cargando categorías…' : 'Selecciona una categoría'}</option>
        {brand && !loading && !active.some(item => item.idCategoria === brand.idCategoria) && <option value={brand.idCategoria} disabled>Categoría actual inactiva; selecciona otra</option>}
        {active.map(item => <option key={item.idCategoria} value={item.idCategoria}>{item.nombre}</option>)}
      </select>
    </div>
    <BrandImagePicker currentImage={!removeImage ? brand?.urlImagen ?? undefined : undefined} url={imageUrl} file={image} onChange={(url, file) => { setImageUrl(url); setImage(file); setRemoveImage(!url && !file) }} disabled={pending} />
    {listError && <div role="alert"><p>{listError}</p><Button onClick={reload}>Reintentar</Button></div>}
    {!loading && !listError && !active.length && <p role="status">Primero crea una categoría en el apartado Categorías.</p>}
    {error && <p className="brand-error" role="alert">{error}</p>}
    <Button type="submit" disabled={pending || loading || !!listError || !active.some(item => item.idCategoria === Number(category))}>
      {pending ? 'Guardando…' : brand ? 'Guardar cambios' : 'Guardar marca'}
    </Button>
  </form>
}




