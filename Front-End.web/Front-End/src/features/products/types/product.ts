export type ProductPresentation = { id_presentacion: number; unidades_equivalentes: number; precio: number }
export type CreateProductRequest = {
  codigo_barra: string
  nombre: string
  idMarca: number
  stock_minimo: number
  urlImagen?: string
  imagen?: File
  presentaciones: ProductPresentation[]
}

export type ProductSummary = {
  idProducto: number; codigoBarra: string | null; nombre: string; marca: string
  idMarca: number; marcaActiva: boolean; idCategoria: number; categoria: string; categoriaActiva: boolean
  imagen: string | null; stockUnidades: number; stockMinimo: number
}
export type ProductPage = { pagina: number; tamanoPagina: number; total: number; items: ProductSummary[] }
export type ProductDetail = ProductSummary & {
  fechaCreacion: string
  presentaciones: {
    idProductoPresentacion: number; idPresentacion: number; descripcion: string | null
    unidadesEquivalentes: number; precio: number; activa: boolean; presentacionActiva: boolean
    presentacionesDisponibles: number
  }[]
}
