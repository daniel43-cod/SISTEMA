import { useEffect, useId, useRef, useState } from 'react'
import { useAuth } from '../../auth'
import { ProductForm } from '../components/ProductForm'
import { SurtidoForm } from '../components/SurtidoForm'
import { ProductCatalogBrowser } from './ProductCatalogBrowser'
import '../../../shared/ui/catalog.css'
import './ProductsPage.css'

export function CreateProductPage() {
  const { session } = useAuth()
  const [mobileView, setMobileView] = useState(() => window.matchMedia('(max-width: 800px)').matches)
  const [createMenuOpen, setCreateMenuOpen] = useState(false)
  const createMenu = useRef<HTMLDivElement>(null)
  const createMenuId = useId()
  useEffect(() => {
    const media = window.matchMedia('(max-width: 800px)')
    const update = () => { setMobileView(media.matches); setCreateMenuOpen(false) }
    media.addEventListener('change', update)
    return () => media.removeEventListener('change', update)
  }, [])
  useEffect(() => {
    if (!createMenuOpen) return
    const closeOutside = (event: PointerEvent) => {
      if (event.target instanceof Node && !createMenu.current?.contains(event.target)) setCreateMenuOpen(false)
    }
    document.addEventListener('pointerdown', closeOutside)
    return () => document.removeEventListener('pointerdown', closeOutside)
  }, [createMenuOpen])
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
    const trigger = !mobileView && editor === 'surtido' ? addSurtido : add
    setEditor(null)
    requestAnimationFrame(() => trigger.current?.focus())
  }
  const actions = mobileView ? <div className="product-mobile-create-actions" ref={createMenu} onKeyDown={event => {
    if (event.key === 'Escape') { event.stopPropagation(); setCreateMenuOpen(false); add.current?.focus() }
  }}>
    {createMenuOpen && <div id={createMenuId} className="product-mobile-create-options" role="group" aria-label="Opciones de creación">
      <button type="button" onClick={() => { setCreateMenuOpen(false); setNotice(''); setEditor('product') }}>Crear producto</button>
      <button type="button" onClick={() => { setCreateMenuOpen(false); setNotice(''); setEditor('surtido') }}>Crear surtido</button>
    </div>}
    <button ref={add} type="button" className="catalog-add-button product-mobile-create-trigger" aria-label={createMenuOpen ? 'Cerrar opciones de creación' : 'Crear producto o surtido'} aria-expanded={createMenuOpen} aria-controls={createMenuId}
      onClick={() => setCreateMenuOpen(open => !open)}><span aria-hidden="true">{createMenuOpen ? '×' : '+'}</span></button>
  </div> : <div className="product-create-actions">
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
        setNotice('Surtido creado correctamente. Puedes verlo dentro de su marca.'); setRevision(value => value + 1); setEditor(null)
        requestAnimationFrame(() => (mobileView ? add : addSurtido).current?.focus())
      }} /> : <ProductForm onSaved={() => { setNotice('Producto creado correctamente.'); setRevision(value => value + 1); close() }} />}
    </section> : <section className="product-card product-create-panel"><ProductCatalogBrowser key={revision} createAction={actions} /></section>}
  </div>
}
