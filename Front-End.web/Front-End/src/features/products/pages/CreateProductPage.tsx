import { useEffect, useRef, useState } from 'react'
import { useAuth } from '../../auth'
import { ProductForm } from '../components/ProductForm'
import { ProductListPage } from './ProductListPage'
import '../../../shared/ui/catalog.css'
import './ProductsPage.css'

export function CreateProductPage() {
  const { session } = useAuth()
  const [open, setOpen] = useState(false)
  const [notice, setNotice] = useState('')
  // Al guardar, cambia la clave para volver a consultar los productos.
  const [revision, setRevision] = useState(0)
  const panel = useRef<HTMLElement>(null)
  const add = useRef<HTMLButtonElement>(null)
  useEffect(() => { if (open) panel.current?.focus() }, [open])
  if (session?.user.role !== 'ADMINISTRADOR') return null
  function close() { setOpen(false); requestAnimationFrame(() => add.current?.focus()) }
  return <div className="catalog-page">
    <p role="status" className="catalog-notice">{notice}</p>
    {open ? <section ref={panel} tabIndex={-1} className="product-card catalog-editor" aria-labelledby="product-title">
      <h2 id="product-title">Crear producto</h2>
      <button type="button" className="catalog-close" aria-label="Cerrar formulario" title="Cerrar formulario" onClick={close}>×</button>
      <ProductForm onSaved={() => { setNotice('Producto creado correctamente.'); setRevision(value => value + 1); close() }} />
    </section> : <>
      {/* El botón + abre el formulario; el listado es la vista principal. */}
      <div className="product-toolbar">
        <button ref={add} type="button" className="catalog-add-button" aria-label="Crear producto" title="Crear producto"
          onClick={() => { setNotice(''); setOpen(true) }}><span aria-hidden="true">+</span></button>
      </div>
      <ProductListPage key={revision} />
    </>}
  </div>
}
