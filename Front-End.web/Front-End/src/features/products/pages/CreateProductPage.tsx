import { useEffect, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import { useAuth } from '../../auth'
import { ProductForm } from '../components/ProductForm'
import { ProductCatalogBrowser } from './ProductCatalogBrowser'
import '../../../shared/ui/catalog.css'
import './ProductsPage.css'

export function CreateProductPage() {
  const { session } = useAuth()
  const [open, setOpen] = useState(false)
  const [notice, setNotice] = useState('')
  // Al guardar, cambia la clave para volver a consultar los productos.
  const [revision, setRevision] = useState(0)
  const panel = useRef<HTMLDialogElement>(null)
  const add = useRef<HTMLButtonElement>(null)
  useEffect(() => {
    if (!open) return
    const dialog = panel.current
    const previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    dialog?.showModal()
    dialog?.focus()
    return () => {
      dialog?.close()
      document.body.style.overflow = previousOverflow
    }
  }, [open])
  if (session?.user.role !== 'ADMINISTRADOR') return null
  function close() { setOpen(false); requestAnimationFrame(() => add.current?.focus()) }
  return <div className="catalog-page">
    <p role="status" className="catalog-notice">{notice}</p>
    {open ? createPortal(<dialog ref={panel} tabIndex={-1} className="product-card catalog-editor product-create-dialog" aria-labelledby="product-title" onCancel={event => { event.preventDefault(); close() }}>
      <h2 id="product-title">Crear producto</h2>
      <button type="button" className="catalog-close" aria-label="Cerrar formulario" title="Cerrar formulario" onClick={close}>×</button>
      <ProductForm onSaved={() => { setNotice('Producto creado correctamente.'); setRevision(value => value + 1); close() }} />
    </dialog>, document.body) : <>
      {/* El botón + abre el formulario; el listado es la vista principal. */}
      <section className="product-card product-create-panel"><ProductCatalogBrowser key={revision} createAction={<button ref={add} type="button" className="catalog-add-button catalog-create-responsive" aria-label="Crear producto" title="Crear producto"
          onClick={() => { setNotice(''); setOpen(true) }}><span aria-hidden="true">+</span><span className="catalog-create-label">Crear producto</span></button>} /></section>
    </>}
  </div>
}
