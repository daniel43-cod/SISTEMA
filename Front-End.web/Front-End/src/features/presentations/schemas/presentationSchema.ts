import type { PresentationResponse } from '../types/presentation.ts'

export function validateDescription(description: string): string | undefined {
  if (!description.trim()) return 'Ingresa una descripción.'
  if (description.length > 100) return 'La descripción admite hasta 100 caracteres.'
}
export function parsePresentation(value: unknown): PresentationResponse {
  if (typeof value !== 'object' || value === null) throw new Error('Respuesta inválida.')
  const data = value as Record<string, unknown>
  if (!Number.isSafeInteger(data.idPresentacion) || Number(data.idPresentacion) <= 0 ||
    typeof data.descripcion !== 'string' || !data.descripcion.trim() || data.descripcion.length > 100 ||
    data.estado !== true) throw new Error('Respuesta inválida.')
  return { idPresentacion: Number(data.idPresentacion), descripcion: data.descripcion, estado: data.estado }
}