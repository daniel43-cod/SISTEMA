import { useId, useState } from 'react'
import type { FormEvent } from 'react'
import { Input } from '../../../shared/ui/Input'
import { Button } from '../../../shared/ui/Button'
import { useCategories } from '../../categories'
import { useCreateBrand } from '../hooks/useCreateBrand'

export function BrandForm({ onSaved }: { onSaved: () => void }) {
  const [name, setName] = useState('')
  const [category, setCategory] = useState('')
  const id = useId()
  const { items, loading, error: listError, reload } = useCategories()
  const { submit, pending, error } = useCreateBrand()
  const active = items.filter(item => item.estado)
  async function save(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!active.some(item => item.idCategoria === Number(category))) return
    if (await submit({ nombre: name, idCategoria: Number(category) })) onSaved()
  }
  return <form className="brand-form" onSubmit={save} aria-busy={pending}>
    <Input label="Nombre de la marca" value={name} onChange={event => setName(event.target.value)}
      required maxLength={100} disabled={pending} placeholder="Ejemplo: Frito-Lay" />
    <div className="field">
      <label htmlFor={id}>Categoría</label>
      <select id={id} className="input" value={category} required disabled={pending || loading || !!listError}
        onChange={event => setCategory(event.target.value)}>
        <option value="">{loading ? 'Cargando categorías…' : 'Selecciona una categoría'}</option>
        {active.map(item => <option key={item.idCategoria} value={item.idCategoria}>{item.nombre}</option>)}
      </select>
    </div>
    {listError && <div role="alert"><p>{listError}</p><Button onClick={reload}>Reintentar</Button></div>}
    {!loading && !listError && !active.length && <p role="status">Primero crea una categoría en el apartado Categorías.</p>}
    {error && <p className="brand-error" role="alert">{error}</p>}
    <Button type="submit" disabled={pending || loading || !!listError || !active.some(item => item.idCategoria === Number(category))}>
      {pending ? 'Guardando…' : 'Guardar marca'}
    </Button>
  </form>
}
