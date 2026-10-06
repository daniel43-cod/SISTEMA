import { useEffect, useRef, useState } from 'react'
import { Input } from '../../../shared/ui/Input'
import { Button } from '../../../shared/ui/Button'

export function BrandImagePicker({ url, file, onChange, disabled }: {
  url: string; file?: File; onChange: (url: string, file?: File) => void; disabled: boolean
}) {
  const [preview, setPreview] = useState('')
  const [failed, setFailed] = useState(false)
  const [error, setError] = useState('')
  const fileRef = useRef<HTMLInputElement>(null)
  const cameraRef = useRef<HTMLInputElement>(null)
  useEffect(() => () => { if (preview) URL.revokeObjectURL(preview) }, [preview])
  function clearImage() {
    onChange(''); setPreview(''); setFailed(false); setError('')
    if (fileRef.current) fileRef.current.value = ''
    if (cameraRef.current) cameraRef.current.value = ''
  }
  function selectFile(selected?: File) {
    if (!selected) return
    if (!['image/jpeg', 'image/png', 'image/webp'].includes(selected.type) ||
        selected.size === 0 || selected.size > 5 * 1024 * 1024) {
      setError('Selecciona una imagen JPEG, PNG o WebP de hasta 5 MB.')
      return
    }
    onChange('', selected); setPreview(URL.createObjectURL(selected)); setFailed(false); setError('')
  }
  const previewUrl = preview || (/^https:\/\//i.test(url.trim()) ? url.trim() : '')
  return (
    <fieldset disabled={disabled} className="brand-image">
      <legend>Imagen (opcional)</legend>
      <Input label="Enlace de la imagen" type="url" value={url} maxLength={2048}
        placeholder="https://ejemplo.com/imagen.jpg"
        onChange={event => { onChange(event.target.value); setPreview(''); setFailed(false); setError('') }} />
      <div className="brand-image-actions">
        <Button className="brand-secondary" onClick={() => { if (fileRef.current) { fileRef.current.value = ""; fileRef.current.click() } }}>Importar imagen</Button>

      </div>
      <input ref={fileRef} type="file" hidden accept="image/jpeg,image/png,image/webp"
        onChange={event => selectFile(event.target.files?.[0])} />
      <input ref={cameraRef} type="file" hidden accept="image/jpeg,image/png,image/webp" capture="environment"
        onChange={event => selectFile(event.target.files?.[0])} />
      
      {file && <p>{file.name}</p>}
      <div className="brand-preview-panel">
        <span className="brand-preview-label">Vista previa</span>
        <button type="button" className="brand-preview-frame" disabled={disabled}
          aria-label={file || url ? "Tomar otra fotografía" : "Tomar fotografía"}
          
          onClick={() => { if (cameraRef.current) { cameraRef.current.value = ""; cameraRef.current.click() } }}>
          {previewUrl && !failed
            ? <img key={previewUrl} className="brand-image-preview" src={previewUrl}
                alt="Imagen seleccionada para la marca" referrerPolicy="no-referrer"
                onError={() => setFailed(true)} />
            : <span className="brand-preview-placeholder">
                <svg width="40" height="40" viewBox="0 0 24 24" fill="none"
                  stroke="currentColor" strokeWidth="1.5" aria-hidden="true">
                  <rect x="3" y="3" width="18" height="18" rx="3" />
                  <circle cx="8" cy="8" r="1.5" />
                  <path d="m3 17 5-5 4 4 4-6 5 7" />
                </svg>
                <span>{failed ? 'Imagen no disponible. Toca para tomar una foto.' : 'Toca para tomar una foto'}</span>
              </span>}
        </button>
      </div>
    
      {(file || url) && <Button className="brand-secondary" onClick={clearImage}>Quitar imagen</Button>}
      <small>JPEG, PNG o WebP, hasta 5 MB.</small>
      {error && <p className="brand-error" role="alert">{error}</p>}
    </fieldset>
  )
}

