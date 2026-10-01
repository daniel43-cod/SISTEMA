export interface CreateCategoryRequest {
  imagen?: File
  nombre: string
  urlImagen?: string | null
}

export interface CategoryResponse {
  idCategoria: number
  nombre: string
  urlImagen?: string | null
  estado: boolean
}

