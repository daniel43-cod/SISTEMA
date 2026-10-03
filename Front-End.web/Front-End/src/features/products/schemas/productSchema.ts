import type { CreateProductRequest } from '../types/product.ts'

function hasControlCharacters(value: string) {
  return Array.from(value).some(character => character.charCodeAt(0) < 32 || character.charCodeAt(0) === 127)
}

// La API vuelve a validar estos datos; aquí evitamos envíos inválidos desde el formulario.
export function validateProduct(data: CreateProductRequest): string | undefined {
  if (!data.codigo_barra.trim() || data.codigo_barra.length > 100 || /\s/.test(data.codigo_barra.trim()) || hasControlCharacters(data.codigo_barra))
    return 'Ingresa un código de barras de hasta 100 caracteres, sin espacios internos.'
  if (!data.nombre.trim() || data.nombre.length > 200 || hasControlCharacters(data.nombre))
    return 'Ingresa un nombre válido de hasta 200 caracteres.'
  if (!Number.isSafeInteger(data.idMarca) || data.idMarca <= 0) return 'Selecciona una marca.'
  if (!Number.isInteger(data.stock_minimo) || data.stock_minimo < 0 || data.stock_minimo > 2147483647)
    return 'El stock mínimo debe ser un entero mayor o igual a cero.'
  if (!data.presentaciones.length || data.presentaciones.length > 100) return 'Agrega entre 1 y 100 presentaciones.'
  const ids = new Set<number>()
  for (const row of data.presentaciones) {
    if (!Number.isSafeInteger(row.id_presentacion) || row.id_presentacion <= 0 || ids.has(row.id_presentacion))
      return 'Selecciona presentaciones válidas sin repetirlas.'
    ids.add(row.id_presentacion)
    if (!Number.isInteger(row.unidades_equivalentes) || row.unidades_equivalentes <= 0 || row.unidades_equivalentes > 2147483647)
      return 'Las unidades equivalentes deben ser enteros mayores que cero.'
    if (!Number.isFinite(row.precio) || row.precio <= 0 || row.precio > 9999999999999.99 ||
        Math.abs(row.precio * 100 - Math.round(row.precio * 100)) > 0.00001)
      return 'El precio debe ser positivo y tener como máximo dos decimales.'
  }
  if (data.imagen && data.urlImagen?.trim()) return 'Elige una imagen o un enlace, no ambos.'
  if (data.urlImagen?.trim()) {
    try {
      const url = new URL(data.urlImagen.trim())
      if (url.protocol !== 'https:' || url.username || url.password || data.urlImagen.trim().length > 2048) throw new Error()
    } catch { return 'Ingresa una URL HTTPS válida para la imagen.' }
  }
  if (data.imagen && (!['image/jpeg', 'image/png', 'image/webp'].includes(data.imagen.type) ||
      data.imagen.size === 0 || data.imagen.size > 5 * 1024 * 1024))
    return 'La imagen debe ser JPEG, PNG o WebP y pesar hasta 5 MB.'
}

