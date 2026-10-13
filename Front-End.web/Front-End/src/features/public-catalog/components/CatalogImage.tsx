import { useState } from 'react'

export function CatalogImage({ url, name, kind }: {
  url: string | null
  name: string
  kind: 'categorias' | 'marcas' | 'productos'
}) {
  const [failed, setFailed] = useState(false)
  const localPattern = kind === 'categorias'
    ? /^\/uploads\/categorias\/[a-zA-Z0-9.-]+$/
    : kind === 'marcas'
      ? /^\/uploads\/marcas\/[a-zA-Z0-9.-]+$/
      : /^\/uploads\/productos\/[a-zA-Z0-9.-]+$/
  let valid = Boolean(url && localPattern.test(url))
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
