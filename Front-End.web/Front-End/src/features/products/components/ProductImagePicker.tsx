import { useEffect, useRef, useState } from 'react'
import { Input } from '../../../shared/ui/Input'

export function ProductImagePicker({ url, file, onChange, disabled }: {
  url: string; file?: File; onChange: (url: string, file?: File) => void; disabled: boolean
}) {
  const [preview, setPreview] = useState('')
  const [failed, setFailed] = useState(false)
  const upload = useRef<HTMLInputElement>(null)
  const camera = useRef<HTMLInputElement>(null)
  useEffect(() => () => { if (preview) URL.revokeObjectURL(preview) }, [preview])
  function select(value?: File) {
    if (!value) return
    setPreview(URL.createObjectURL(value)); setFailed(false); onChange('', value)
  }
  const source = preview || (/^https:\/\//i.test(url) ? url : '')
  return <fieldset disabled={disabled} className="product-image">
    <legend>Imagen (opcional)</legend>
    <Input label="Enlace de la imagen" type="url" value={url} maxLength={2048}
      onChange={event => { setPreview(''); setFailed(false); onChange(event.target.value) }} />
    <button type="button" className="product-preview" aria-label="Tomar fotografía"
      onClick={() => { if (camera.current) { camera.current.value = ''; camera.current.click() } }}>
      {source && !failed ? <img key={source} src={source} alt="Vista previa del producto"
        referrerPolicy="no-referrer" onError={() => setFailed(true)} />
        : <span>{failed ? 'Imagen no disponible' : 'Toca para tomar una foto'}</span>}
    </button>
    <div className="product-actions">
      <button type="button" onClick={() => { if (upload.current) { upload.current.value = ''; upload.current.click() } }}>Importar imagen</button>
      {(url || file) && <button type="button" onClick={() => { setPreview(''); setFailed(false); onChange('') }}>Quitar imagen</button>}
    </div>
    <input hidden ref={upload} type="file" accept="image/jpeg,image/png,image/webp" onChange={event => select(event.target.files?.[0])} />
    <input hidden ref={camera} type="file" accept="image/jpeg,image/png,image/webp" capture="environment" onChange={event => select(event.target.files?.[0])} />
    <small>JPEG, PNG o WebP, hasta 5 MB. La cámara depende del dispositivo.</small>
    {file && <small>{file.name}</small>}
  </fieldset>
}
