export interface CreateBrandRequest { nombre: string; idCategoria: number }
export interface BrandResponse extends CreateBrandRequest { idMarca: number; estado: boolean }
