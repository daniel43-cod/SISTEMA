import { useRef, useState } from 'react'
import './catalog-row.css'

export function RowDetails({ name, type, fields, image, disabled }: {
  name: string; type: string; fields: { label: string; value: string }[]; image?: string | null; disabled: boolean
}) {
  const dialog = useRef<HTMLDialogElement>(null)
  const [failed, setFailed] = useState(false)
  const source = image?.trim() ?? ''
  let safe = /^\/uploads\/(?:marcas|categorias)\/[a-zA-Z0-9_-]+\.(?:webp|png|jpe?g)$/i.test(source)
  if (source.startsWith('https://')) {
    try { const url = new URL(source); safe = Boolean(url.hostname) && !url.username && !url.password }
    catch { safe = false }
  }
  return <>
    <button type="button" className="catalog-row-open" disabled={disabled}
      aria-label={`Ver detalles de ${name}`} aria-haspopup="dialog" onClick={() => dialog.current?.showModal()} />
    <dialog ref={dialog} className="catalog-row-dialog" aria-label={`Detalles de ${type}: ${name}`}>
      <div className="catalog-row-dialog-heading"><h2>Detalles de {type}</h2>
        <button type="button" autoFocus onClick={() => dialog.current?.close()} aria-label="Cerrar detalles">×</button></div>
      <h3>{name}</h3>
      {image !== undefined && (safe && !failed
        ? <img key={source} className="catalog-row-dialog-image" src={source} alt={`Imagen de ${name}`} referrerPolicy="no-referrer" onError={() => setFailed(true)} />
        : <p>Sin imagen</p>)}
      <dl>{fields.map(field => <div key={field.label}><dt>{field.label}</dt><dd>{field.value}</dd></div>)}</dl>
      <button type="button" onClick={() => dialog.current?.close()}>Cerrar</button>
    </dialog>
  </>
}
