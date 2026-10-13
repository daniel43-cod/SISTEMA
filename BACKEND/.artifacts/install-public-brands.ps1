$ErrorActionPreference = 'Stop'
$frontendRoot = 'C:\Users\coy81\Desktop\SISTEMA\SISTEMA\Front-End.web\Front-End'
$manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'public-brands-files.json') -Raw | ConvertFrom-Json
$utf8 = New-Object System.Text.UTF8Encoding($false)
$pagePath = Join-Path $frontendRoot 'src/features/public-catalog/pages/PublicCategoriesPage.tsx'
$page = [System.IO.File]::ReadAllText($pagePath).Replace("`r`n", "`n")
$originalPage = $page
$imageStart = $page.IndexOf('function CategoryImage(')
$imageEnd = $page.IndexOf('export function PublicCategoriesPage()')
if ($imageStart -lt 0 -or $imageEnd -le $imageStart) { throw 'Revisar componente de imagen antes de editar.' }
$page = $page.Remove($imageStart, $imageEnd - $imageStart)
$page = $page.Replace("import { useEffect, useState } from 'react'", "import { useEffect, useRef, useState } from 'react'")
$page = "import type { PublicCategory } from '../types/category'`nimport { CatalogImage } from '../components/CatalogImage'`nimport { PublicBrandsList } from '../components/PublicBrandsList'`n" + $page
$page = $page.Replace('  const { items, loading, error, reload } = usePublicCategories()', @'
  const { items, loading, error, reload } = usePublicCategories()
  const [selectedCategory, setSelectedCategory] = useState<PublicCategory | null>(null)
  const headingRef = useRef<HTMLHeadingElement>(null)
  const navigationStarted = useRef(false)
  useEffect(() => {
    if (navigationStarted.current) headingRef.current?.focus()
    navigationStarted.current = true
  }, [selectedCategory])
'@)
$page = $page.Replace('Ir a las categorías', 'Ir al catálogo')
$page = $page.Replace('<h1>Categorías disponibles</h1>', '<h1 ref={headingRef} tabIndex={-1}>{selectedCategory ? `Marcas de ${selectedCategory.nombre}` : ''Categorías disponibles''}</h1>')
$page = $page.Replace('<p className="public-catalog-description">Conoce nuestras categorías sin crear una cuenta.</p>', @'
      <p className="public-catalog-description">{selectedCategory
        ? 'Conoce las marcas disponibles en esta categoría.'
        : 'Conoce nuestras categorías sin crear una cuenta.'}</p>
      {selectedCategory && <button type="button" className="public-catalog-back"
        onClick={() => setSelectedCategory(null)}>← Volver a las categorías</button>}
      {selectedCategory
        ? <PublicBrandsList key={selectedCategory.idCategoria} idCategoria={selectedCategory.idCategoria} />
        : <>
'@)
$page = $page.Replace('<CategoryImage key={category.urlImagen} url={category.urlImagen} name={category.nombre} />', '<button type="button" className="public-category-select" onClick={() => setSelectedCategory(category)}>`n          <CatalogImage key={category.urlImagen} url={category.urlImagen} name={category.nombre} kind="categorias" />')
# Replace the literal marker with a newline without changing user text.
$page = $page.Replace('>`n          <CatalogImage', ">`n          <CatalogImage")
$page = $page.Replace('<h2>{category.nombre}</h2>', '<h2>{category.nombre}</h2></button>')
$page = $page.Replace('    </main>', '      </>}`n    </main>').Replace('</>}`n    </main>', "</>}`n    </main>")
if ($page -eq $originalPage -or $page.Contains('<CategoryImage')) { throw 'No se pudo integrar la navegación.' }
$apiPath = Join-Path $frontendRoot 'src/features/public-catalog/api/publicCatalogApi.ts'
$api = [System.IO.File]::ReadAllText($apiPath)
if ($api.Contains('listPublicBrands')) { throw 'El listado de marcas ya existe.' }
$api = "import type { PublicBrand } from '../types/brand'`n" + $api + $manifest.apiAppend
$cssPath = Join-Path $frontendRoot 'src/features/public-catalog/pages/PublicCategoriesPage.css'
$css = [System.IO.File]::ReadAllText($cssPath) + $manifest.cssAppend
foreach ($entry in $manifest.files.PSObject.Properties) {
    $target = Join-Path $frontendRoot $entry.Name
    if (Test-Path -LiteralPath $target) { throw "El archivo ya existe: $target" }
}
foreach ($entry in $manifest.files.PSObject.Properties) {
    $target = Join-Path $frontendRoot $entry.Name
    [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($target)) | Out-Null
    [System.IO.File]::WriteAllText($target, [string]$entry.Value, $utf8)
}
[System.IO.File]::WriteAllText($pagePath, $page, $utf8)
[System.IO.File]::WriteAllText($apiPath, $api, $utf8)
[System.IO.File]::WriteAllText($cssPath, $css, $utf8)
Write-Output 'Navegación de categorías a marcas integrada.'
