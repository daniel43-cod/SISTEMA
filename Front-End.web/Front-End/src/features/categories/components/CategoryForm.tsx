import { useEffect, useId, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { Input } from '../../../shared/ui/Input'
import { Button } from '../../../shared/ui/Button'
import { useCreateCategory } from '../hooks/useCreateCategory'

export function CategoryForm() {
  const [name, setName] = useState('')
  const [url, setUrl] = useState('')
  const [file, setFile] = useState<File>()
  const [preview, setPreview] = useState('')
  const [failed, setFailed] = useState(false)
  const fileRef = useRef<HTMLInputElement>(null)
  const cameraRef = useRef<HTMLInputElement>(null)
  const helpId = useId()
  const { submit, pending, fieldError, error, success } = useCreateCategory()
  useEffect(() => () => { if (preview) URL.revokeObjectURL(preview) }, [preview])
  function clearImage() {
    setFile(undefined); setPreview(''); setUrl(''); setFailed(false)
    if (fileRef.current) fileRef.current.value = ''
    if (cameraRef.current) cameraRef.current.value = ''
  }
  function selectFile(selected?: File) {
    if (!selected) return
    setFile(selected); setPreview(URL.createObjectURL(selected)); setUrl(''); setFailed(false)
  }
  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (await submit(name, url, file)) { setName(''); clearImage() }
  }
  const previewUrl = preview || (/^https:\/\//i.test(url.trim()) ? url.trim() : '')
  return <form className="category-form" onSubmit={handleSubmit} noValidate aria-busy={pending}>
    <Input label="Nombre de la categoría" name="nombre" value={name}
      onChange={event => setName(event.target.value)} placeholder="Ejemplo: Bebidas"
      required maxLength={100} disabled={pending} error={fieldError} aria-describedby={helpId} />
    <p id={helpId} className="category-hint">Agrupa tus productos: bebidas, abarrotes, limpieza, etc.</p>
    <fieldset disabled={pending} className="category-image">
      <legend>Imagen (opcional)</legend>
      <Input label="Enlace de la imagen" type="url" value={url} maxLength={2048}
        placeholder="https://ejemplo.com/imagen.jpg"
        onChange={event => { setUrl(event.target.value); setFile(undefined); setPreview(''); setFailed(false) }} />
      <div className="category-image-actions">
        <Button onClick={() => fileRef.current?.click()}>Importar imagen local</Button>
        <Button onClick={() => cameraRef.current?.click()}>Tomar fotografía</Button>
      </div>
      <input ref={fileRef} type="file" hidden accept="image/jpeg,image/png,image/webp"
        onChange={event => selectFile(event.target.files?.[0])} />
      <input ref={cameraRef} type="file" hidden accept="image/jpeg,image/png,image/webp" capture="environment"
        onChange={event => selectFile(event.target.files?.[0])} />
      <p className="category-hint">JPEG, PNG o WebP, hasta 5 MB. La cámara depende del dispositivo y navegador.</p>
      {file && <p>{file.name}</p>}
      <div className="category-preview-panel">
        <span className="category-preview-label">Vista previa</span>
        <div className="category-preview-frame">
          {previewUrl && !failed
            ? <img key={previewUrl} className="category-image-preview" src={previewUrl}
                alt="Imagen seleccionada para la categoría" referrerPolicy="no-referrer"
                onError={() => setFailed(true)} />
            : <div className="category-preview-placeholder">
                <svg width="40" height="40" viewBox="0 0 24 24" fill="none"
                  stroke="currentColor" strokeWidth="1.5" aria-hidden="true">
                  <rect x="3" y="3" width="18" height="18" rx="3" />
                  <circle cx="8" cy="8" r="1.5" />
                  <path d="m3 17 5-5 4 4 4-6 5 7" />
                </svg>
                <p role="status">{failed ? 'Imagen no disponible.' : 'Aquí aparecerá la imagen de la categoría.'}</p>
              </div>}
        </div>
      </div>
      {(file || url) && <Button onClick={clearImage}>Quitar imagen</Button>}
    </fieldset>
    {fieldError && <p className="sr-only" role="alert">{fieldError}</p>}
    {error && <p className="category-error" role="alert">{error}</p>}
    <p className="category-success" role="status">{success}</p>
    <Button type="submit" disabled={pending}>{pending ? 'Guardando…' : 'Guardar categoría'}</Button>
  </form>
}


