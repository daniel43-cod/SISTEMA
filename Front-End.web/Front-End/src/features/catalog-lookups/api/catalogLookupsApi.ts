import { listCategories } from '../../categories/api/categoriesApi.ts'
import { listBrands } from '../../brands/api/brandsApi.ts'
import { readCatalogLookupCache } from '../../../shared/cache/catalogLookupCache.ts'

export interface CatalogCategoryOption { idCategoria: number; nombre: string; estado: boolean }
export interface CatalogBrandOption { idMarca: number; nombre: string; idCategoria: number; estado: boolean }
export interface CatalogLookups { categories: CatalogCategoryOption[]; brands: CatalogBrandOption[] }

export function loadCatalogLookups(token: string): Promise<CatalogLookups> {
  return readCatalogLookupCache(token, async () => {
    const [categories, brands] = await Promise.all([
      listCategories(token, undefined, true),
      listBrands(token, undefined, true),
    ])
    return {
      categories: categories.map(({ idCategoria, nombre, estado }) => ({ idCategoria, nombre, estado })),
      brands: brands.map(({ idMarca, nombre, idCategoria, estado }) => ({ idMarca, nombre, idCategoria, estado })),
    }
  })
}
