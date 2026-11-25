export interface PublicPresentation {
  idProductoPresentacion: number
  nombre: string
  precio: number
  disponible: boolean
}
export interface PublicProduct {
  idProducto: number
  nombre: string
  urlImagen: string | null
  presentaciones: PublicPresentation[]
}
export interface PublicProductsPage {
  pagina: number
  tamanoPagina: number
  hayMas: boolean
  productos: PublicProduct[]
}
export interface CartItem {
  idProducto: number
  idProductoPresentacion: number
  producto: string
  presentacion: string
  precio: number
  cantidad: number
  idSurtido?: number
  unidadesEquivalentes?: number
  componentes?: { idProducto: number; nombre: string; cantidad: number }[]
}

export type PublicSurtido = {
  idSurtido: number; nombre: string; urlImagen: string | null
  productos: { idProducto: number; nombre: string; disponible: boolean }[]
  presentaciones: (PublicPresentation & { unidadesEquivalentes: number })[]
}
export type SurtidoCatalogSelection = { idProductoPresentacion: number; cantidad: number;
  unidadesEquivalentes: number; componentes: { idProducto: number; nombre: string; cantidad: number }[] }
