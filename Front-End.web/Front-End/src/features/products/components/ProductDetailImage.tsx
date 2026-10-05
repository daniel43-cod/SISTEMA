import { useState } from 'react'

export function ProductDetailImage({ image, name }: { image: string | null; name: string }) {
  const [failed, setFailed] = useState(false)
  const source = typeof image === 'string' ? image.trim() : ''
  let valid = /^\/uploads\/productos\/[a-zA-Z0-9_-]+\.(?:webp|png|jpe?g)$/i.test(source)
  if (!valid && source.startsWith('https://')) {
    try {
      const url = new URL(source)
      valid = Boolean(url.hostname) && !url.username && !url.password
    } catch { valid = false }
  }
  return <div className="product-detail-image">
    {valid && !failed
      ? <img src={source} alt={`Imagen de ${name}`} referrerPolicy="no-referrer" onError={() => setFailed(true)} />
      : <span>Sin imagen</span>}
  </div>
}