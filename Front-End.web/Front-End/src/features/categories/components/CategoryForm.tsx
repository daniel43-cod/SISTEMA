import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { Input } from '../../../shared/ui/Input'
import { Button } from '../../../shared/ui/Button'
import { useSaveCategory } from '../hooks/useSaveCategory'
import type { CategoryResponse } from '../types/category'

export function CategoryForm({ category, onSaved, onCancel }: { category?: CategoryResponse; onSaved?: () => void; onCancel?: () => void }) {
  const [name, setName] = useState(category?.nombre ?? '')
  const [removeImage, setRemoveImage] = useState(false)
  const [url, setUrl] = useState('')
  const [file, setFile] = useState<File>()
  const [preview, setPreview] = useState('')
  const [failed, setFailed] = useState(false)
  const fileRef = useRef<HTMLInputElement>(null)
  const cameraRef = useRef<HTMLInputElement>(null)


  const { submit, pending, fieldError, error, success } = useSaveCategory(category?.idCategoria)
  useEffect(() => () => { if (preview) URL.revokeObjectURL(preview) }, [preview])
  function clearImage() {
    setRemoveImage(true)
    setFile(undefined); setPreview(''); setUrl(''); setFailed(false)
    if (fileRef.current) fileRef.current.value = ''
    if (cameraRef.current) cameraRef.current.value = ''
  }
  function selectFile(selected?: File) {
    if (!selected) return
    setRemoveImage(false); setFile(selected); setPreview(URL.createObjectURL(selected)); setUrl(''); setFailed(false)
  }
  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (await submit(name, url, file, !!category && removeImage && !file && !url.trim())) {
      if (!category) { setName(''); clearImage() }
      onSaved?.()
    }
  }
  const existingImage = !removeImage && !url && !file ? category?.urlImagen : ''
  const safeExisting = existingImage && (/^https:\/\//i.test(existingImage) || /^\/uploads\/categorias\/[a-zA-Z0-9.-]+$/.test(existingImage)) ? existingImage : ''
  const previewUrl = preview || safeExisting || (/^https:\/\//i.test(url.trim()) ? url.trim() : '')
  return <form className="category-form" onSubmit={handleSubmit} noValidate aria-busy={pending}>
    <Input label="Nombre de la categoría" name="nombre" value={name}
      onChange={event => setName(event.target.value)} placeholder="Ejemplo: Bebidas"
      required maxLength={100} disabled={pending} error={fieldError}  />
   
    <fieldset disabled={pending} className="category-image">
      <legend>Imagen (opcional)</legend>
      <Input label="Enlace de la imagen" type="url" value={url} maxLength={2048}
        placeholder="https://ejemplo.com/imagen.jpg"
        onChange={event => { setUrl(event.target.value); setFile(undefined); setPreview(''); setFailed(false) }} />
      <div className="category-image-actions">
        <Button className="category-secondary" onClick={() => { if (fileRef.current) { fileRef.current.value = ""; fileRef.current.click() } }}>Importar imagen</Button>

      </div>
      <input ref={fileRef} type="file" hidden accept="image/jpeg,image/png,image/webp"
        onChange={event => selectFile(event.target.files?.[0])} />
      <input ref={cameraRef} type="file" hidden accept="image/jpeg,image/png,image/webp" capture="environment"
        onChange={event => selectFile(event.target.files?.[0])} />
      
      {file && <p>{file.name}</p>}
      <div className="category-preview-panel">
        <span className="category-preview-label">Vista previa</span>
        <button type="button" className="category-preview-frame" disabled={pending}
          aria-label={file || url ? "Tomar otra fotografía" : "Tomar fotografía"}
          
          onClick={() => { if (cameraRef.current) { cameraRef.current.value = ""; cameraRef.current.click() } }}>
          {previewUrl && !failed
            ? <img key={previewUrl} className="category-image-preview" src={previewUrl}
                alt="Imagen seleccionada para la categoría" referrerPolicy="no-referrer"
                onError={() => setFailed(true)} />
            : <span className="category-preview-placeholder">
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
    
      {(file || url || existingImage) && <Button className="category-secondary" onClick={clearImage}>Quitar imagen</Button>}
    </fieldset>
    {fieldError && <p className="sr-only" role="alert">{fieldError}</p>}
    {error && <p className="category-error" role="alert">{error}</p>}
    <p className="category-success" role="status">{success}</p>
    <Button type="submit" disabled={pending}>{pending ? 'Guardando…' : category ? 'Guardar cambios' : 'Guardar categoría'}</Button>
    {onCancel && <Button className="category-secondary" disabled={pending} onClick={onCancel}>Cancelar edición</Button>}
  </form>
}




