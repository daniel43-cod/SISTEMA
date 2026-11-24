import { useEffect, useRef, useState } from 'react'
import { useAuth } from '../../auth'
import { ProductForm } from '../components/ProductForm'
import { SurtidoForm } from '../components/SurtidoForm'
import { ProductCatalogBrowser } from './ProductCatalogBrowser'
import '../../../shared/ui/catalog.css'
import './ProductsPage.css'

export function CreateProductPage() {
  const { session } = useAuth()
  const [editor, setEditor] = useState<'product' | 'surtido' | null>(null)
  const [surtidoPending, setSurtidoPending] = useState(false)
  const [notice, setNotice] = useState('')
  const [revision, setRevision] = useState(0)
  const panel = useRef<HTMLElement>(null)
  const add = useRef<HTMLButtonElement>(null)
  const addSurtido = useRef<HTMLButtonElement>(null)
  useEffect(() => { if (editor) panel.current?.focus() }, [editor])
  if (session?.user.role !== 'ADMINISTRADOR') return null
  function close() {
    if (surtidoPending) return
    const trigger = editor === 'surtido' ? addSurtido : add
    setEditor(null)
    requestAnimationFrame(() => trigger.current?.focus())
  }
  const actions = <div className="product-create-actions">
    <button ref={add} type="button" className="catalog-add-button catalog-create-responsive" aria-label="Crear producto" title="Crear producto"
      onClick={() => { setNotice(''); setEditor('product') }}><span aria-hidden="true">+</span><span className="catalog-create-label">Crear producto</span></button>
    <button ref={addSurtido} type="button" className="catalog-add-button surtido-create-button" onClick={() => { setNotice(''); setEditor('surtido') }}>
      <span aria-hidden="true">+</span><span>Crear surtido</span></button>
  </div>
  return <div className="catalog-page">
    <p role="status" className="catalog-notice">{notice}</p>
    {editor ? <section ref={panel} tabIndex={-1} className="product-card catalog-editor product-create-editor" aria-labelledby="product-title"
      onKeyDown={event => { if (event.key === 'Escape') { event.stopPropagation(); close() } }}>
      <h2 id="product-title">{editor === 'surtido' ? 'Crear surtido' : 'Crear producto'}</h2>
      <button type="button" className="catalog-close" aria-label="Cerrar formulario" title="Cerrar formulario" disabled={surtidoPending} onClick={close}>×</button>
      {editor === 'surtido' ? <SurtidoForm onPending={setSurtidoPending} onCancel={close} onSaved={() => {
        setNotice('Surtido creado correctamente.'); setRevision(value => value + 1); setEditor(null)
        requestAnimationFrame(() => addSurtido.current?.focus())
      }} /> : <ProductForm onSaved={() => { setNotice('Producto creado correctamente.'); setRevision(value => value + 1); close() }} />}
    </section> : <section className="product-card product-create-panel"><ProductCatalogBrowser key={revision} createAction={actions} /></section>}
  </div>
}
