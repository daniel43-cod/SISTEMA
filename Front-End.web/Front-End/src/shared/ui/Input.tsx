import { useId } from 'react'
import type { InputHTMLAttributes } from 'react'
import './controls.css'

type InputProps = InputHTMLAttributes<HTMLInputElement> & {
  label: string
  error?: string
}
export function Input({ label, error, id, className = '', 'aria-describedby': describedBy, ...props }: InputProps) {
  const generatedId = useId()
  const inputId = id ?? generatedId
  const description = [describedBy, error ? `${inputId}-error` : undefined].filter(Boolean).join(' ') || undefined
  return (
    <div className="field">
      <label htmlFor={inputId}>{label}</label>
      <input {...props} id={inputId} className={`input ${className}`}
        aria-invalid={error ? true : props['aria-invalid']} aria-describedby={description} />
      {error && <p id={`${inputId}-error`} className="field-error">{error}</p>}
    </div>
  )
}