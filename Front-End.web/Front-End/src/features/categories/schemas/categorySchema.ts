import type { CategoryResponse } from '../types/category.ts'

export function validateCategoryName(name: string): string | undefined {
  if (!name.trim()) return 'Ingresa el nombre de la categoría.'
  if (name.trim().length > 100) return 'El nombre admite hasta 100 caracteres.'
}

export function parseCategory(value: unknown): CategoryResponse {
  if (typeof value !== 'object' || value === null) throw new Error('Respuesta inválida')
  const data = value as Record<string, unknown>
  if (!Number.isSafeInteger(data.idCategoria) || (data.idCategoria as number) <= 0 ||
      typeof data.nombre !== 'string' || validateCategoryName(data.nombre) ||
      typeof data.estado !== 'boolean') throw new Error('Respuesta inválida')
  if (data.urlImagen != null && typeof data.urlImagen !== 'string') throw new Error('Respuesta inválida')
  return { ...(typeof data.urlImagen === 'string' ? { urlImagen: data.urlImagen } : {}), idCategoria: data.idCategoria as number, nombre: data.nombre, estado: data.estado }
}

