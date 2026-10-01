import { useEffect, useRef, useState } from 'react'
import { useAuth } from '../../auth'
import { useBrands } from '../hooks/useBrands'
import { BrandList } from '../components/BrandList'
import { BrandForm } from '../components/BrandForm'
import '../../../shared/ui/catalog.css'
import './BrandsPage.css'

export function BrandsPage() {
  const { session } = useAuth()
  const { items, loading, error, reload } = useBrands()
  const [open, setOpen] = useState(false)
  const [notice, setNotice] = useState('')
  const panel = useRef<HTMLElement>(null)
  const add = useRef<HTMLButtonElement>(null)
  useEffect(() => { if (open) panel.current?.focus() }, [open])
  if (session?.user.role !== 'ADMINISTRADOR') return null
  function close() { setOpen(false); requestAnimationFrame(() => add.current?.focus()) }
  return <div className="catalog-page">
    <p className="catalog-notice" role="status">{notice}</p>
    {open ? <section ref={panel} tabIndex={-1} className="brand-card catalog-editor" aria-labelledby="brand-title">
      <button type="button" className="catalog-close" aria-label="Cerrar formulario" title="Cerrar formulario" onClick={close}>×</button>
      <h2 id="brand-title">Crear marca</h2>
      <BrandForm onSaved={() => { setNotice('Marca creada correctamente.'); reload(); close() }} />
    </section> : <>
      {/* El listado se oculta al abrir el formulario y se recarga después de guardar. */}
      <BrandList items={items} loading={loading} error={error} reload={reload} />
      <button type="button" ref={add} className="catalog-add-button" aria-label="Crear marca" title="Crear marca"
        onClick={() => { setNotice(''); setOpen(true) }}><span aria-hidden="true">+</span></button>
    </>}
  </div>
}

