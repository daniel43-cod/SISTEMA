import { useEffect, useState } from 'react'
import { usePublicCategories } from '../hooks/usePublicCategories'
import './PublicCategoriesPage.css'

function CategoryImage({ url, name }: { url: string | null; name: string }) {
  const [failed, setFailed] = useState(false)
  let valid = Boolean(url && /^\/uploads\/categorias\/[a-zA-Z0-9.-]+$/.test(url))
  if (url?.startsWith('https://')) {
    try {
      const parsed = new URL(url)
      valid = Boolean(parsed.hostname) && !parsed.username && !parsed.password
    } catch { valid = false }
  }
  return <div className="public-category-image">
    {valid && !failed
      ? <img src={url!} alt="" loading="lazy" referrerPolicy="no-referrer" onError={() => setFailed(true)} />
      : <div className="public-category-placeholder">
          <span aria-hidden="true">{name.trim().slice(0, 2).toLocaleUpperCase('es')}</span>
          <small>Sin imagen</small>
        </div>}
  </div>
}

export function PublicCategoriesPage() {
  const { items, loading, error, reload } = usePublicCategories()
  useEffect(() => {
    const previous = document.title
    document.title = 'Catálogo | Sistema'
    return () => { document.title = previous }
  }, [])
  return <div className="public-catalog">
    <a className="public-catalog-skip" href="#public-categories">Ir a las categorías</a>
    <header className="public-catalog-header">
      <a className="public-catalog-brand" href={`${import.meta.env.BASE_URL}catalogo`}>
        <img src={`${import.meta.env.BASE_URL}icono_principal.jpeg`} alt="" width="44" height="44" />
        <span>Sistema<small>CATÁLOGO PÚBLICO</small></span>
      </a>
    </header>
    <main id="public-categories" className="public-catalog-main">
      <p className="public-catalog-eyebrow">EXPLORA NUESTROS PRODUCTOS</p>
      <h1>Categorías disponibles</h1>
      <p className="public-catalog-description">Conoce nuestras categorías sin crear una cuenta.</p>
      {loading && <p className="public-catalog-message" role="status">Cargando categorías…</p>}
      {!loading && error && <div className="public-catalog-message" role="alert">
        <p>{error}</p><button type="button" onClick={reload}>Reintentar</button>
      </div>}
      {!loading && !error && items.length === 0 &&
        <p className="public-catalog-message" role="status">Por ahora no hay categorías disponibles.</p>}
      {!loading && !error && items.length > 0 && <ul className="public-categories-grid">
        {items.map(category => <li key={category.idCategoria} className="public-category-card">
          <CategoryImage key={category.urlImagen} url={category.urlImagen} name={category.nombre} />
          <h2>{category.nombre}</h2>
        </li>)}
      </ul>}
    </main>
    <footer className="public-catalog-footer">Sistema · Catálogo público</footer>
  </div>
}
