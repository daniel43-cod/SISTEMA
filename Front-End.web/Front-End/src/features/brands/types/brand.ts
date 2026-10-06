export interface CreateBrandRequest { nombre: string; idCategoria: number; quitarImagen?: boolean; imagen?: File; urlImagen?: string | null }
export interface BrandResponse { idMarca: number; nombre: string; idCategoria: number; estado: boolean; nombreCategoria?: string | null; urlImagen?: string | null }

