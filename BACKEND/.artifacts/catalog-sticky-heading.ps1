$ErrorActionPreference = 'Stop'
$frontendRoot = 'C:\Users\coy81\Desktop\SISTEMA\SISTEMA\Front-End.web\Front-End'
$pagePath = Join-Path $frontendRoot 'src/features/public-catalog/pages/PublicCategoriesPage.tsx'
$cssPath = Join-Path $frontendRoot 'src/features/public-catalog/pages/PublicCategoriesPage.css'
$page = [System.IO.File]::ReadAllText($pagePath).Replace("`r`n", "`n")
$oldHeading = @'
      <p className="public-catalog-eyebrow">EXPLORA NUESTROS PRODUCTOS</p>
      <h1 ref={headingRef} tabIndex={-1}>{selectedBrand ? `Productos de ${selectedBrand.nombre}`
        : selectedCategory ? `Marcas de ${selectedCategory.nombre}` : 'Categorías disponibles'}</h1>
'@
$newHeading = @'
      <div className={selectedBrand ? 'public-catalog-heading public-catalog-heading-sticky' : 'public-catalog-heading'}>
        <p className="public-catalog-eyebrow">EXPLORA NUESTROS PRODUCTOS</p>
        <h1 ref={headingRef} tabIndex={-1}>{selectedBrand ? `Productos de ${selectedBrand.nombre}`
          : selectedCategory ? `Marcas de ${selectedCategory.nombre}` : 'Categorías disponibles'}</h1>
      </div>
'@
$oldDescription = @'
      <p className="public-catalog-description">{selectedBrand
        ? 'Elige tus presentaciones y usa + para agregarlas al carrito.'
        : selectedCategory ? 'Conoce las marcas disponibles en esta categoría.'
        : 'Conoce nuestras categorías sin crear una cuenta.'}</p>
'@
$newDescription = @'
      {!selectedBrand && <p className="public-catalog-description">{selectedCategory
        ? 'Conoce las marcas disponibles en esta categoría.'
        : 'Conoce nuestras categorías sin crear una cuenta.'}</p>}
'@
if (-not $page.Contains($oldHeading) -or -not $page.Contains($oldDescription)) { throw 'El encabezado cambió; revisar antes de editar.' }
$page = $page.Replace($oldHeading, $newHeading).Replace($oldDescription, $newDescription)
$styles = @'

.public-catalog-heading-sticky {
  position: sticky;
  top: 0;
  z-index: 5;
  padding: 12px 0;
  margin-bottom: 20px;
  background: #f5f7fa;
  border-bottom: 1px solid #e1e7ed;
}
.public-catalog-heading-sticky h1 { overflow-wrap: anywhere; }
'@
$utf8 = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($pagePath, $page, $utf8)
[System.IO.File]::AppendAllText($cssPath, $styles, $utf8)
