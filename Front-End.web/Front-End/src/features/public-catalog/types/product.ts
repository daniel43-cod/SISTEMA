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
}
