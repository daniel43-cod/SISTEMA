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
