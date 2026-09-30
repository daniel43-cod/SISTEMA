import { useState } from 'react'
import type { FormEvent } from 'react'
import type { PresentationResponse } from '../types/presentation'
import { Input } from '../../../shared/ui/Input'
import { Button } from '../../../shared/ui/Button'
import { useUpdatePresentation } from '../hooks/useUpdatePresentation'

type Props = { presentation: PresentationResponse; onSaved: () => void; onCancel: () => void }
export function EditPresentationForm({ presentation, onSaved, onCancel }: Props) {
  const [description, setDescription] = useState(presentation.descripcion)
  const { submit, pending, fieldError, error } = useUpdatePresentation()
  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (await submit(presentation.idPresentacion, description)) onSaved()
  }
  return <form className="presentation-form" onSubmit={handleSubmit} noValidate aria-busy={pending}>
    <Input label="Descripción" name="descripcion" value={description} onChange={event => setDescription(event.target.value)}
      required maxLength={100} disabled={pending} error={fieldError} />
    {fieldError && <span className="sr-only" role="alert">{fieldError}</span>}
    {error && <p className="presentation-error" role="alert">{error}</p>}
    <div className="presentation-edit-actions">
      <Button type="submit" disabled={pending}>{pending ? 'Guardando…' : 'Guardar cambios'}</Button>
      <button type="button" className="presentation-refresh" disabled={pending} onClick={onCancel}>Cancelar</button>
    </div>
  </form>
}