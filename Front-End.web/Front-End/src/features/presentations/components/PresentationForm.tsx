import { useState } from 'react'
import type { FormEvent } from 'react'
import { Input } from '../../../shared/ui/Input'
import { Button } from '../../../shared/ui/Button'
import { useCreatePresentation } from '../hooks/useCreatePresentation'

export function PresentationForm({ onCreated }: { onCreated: () => void }) {
  const [description, setDescription] = useState('')
  const { submit, pending, fieldError, error, success } = useCreatePresentation()
  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (await submit(description)) { setDescription(''); onCreated() }
  }
  return <form className="presentation-form" onSubmit={handleSubmit} noValidate aria-busy={pending}>
    <Input label="Descripción" name="descripcion" value={description} onChange={event => setDescription(event.target.value)}
      placeholder="Ejemplo: Caja" required maxLength={100} disabled={pending} error={fieldError}
      aria-describedby="presentation-description-help" />
    <p id="presentation-description-help" className="presentation-hint">Unidad, bandeja, fardo, docena, etc.</p>
    {fieldError && <p className="sr-only" role="alert">{fieldError}</p>}
    {error && <p className="presentation-error" role="alert">{error}</p>}
    <p className="presentation-success" role="status">{success}</p>
    <Button type="submit" disabled={pending}>{pending ? 'Guardando…' : 'Guardar presentación'}</Button>
  </form>
}