$ErrorActionPreference = 'Stop'
$frontendRoot = 'C:\Users\coy81\Desktop\SISTEMA\SISTEMA\Front-End.web\Front-End'
$pagePath = Join-Path $frontendRoot 'src/features/public-catalog/pages/PublicCategoriesPage.tsx'
$cssPath = Join-Path $frontendRoot 'src/features/public-catalog/pages/PublicCategoriesPage.css'
$page = [System.IO.File]::ReadAllText($pagePath).Replace("`r`n", "`n")
$oldButton = @'
      {selectedCategory && <button type="button" className="public-catalog-back"
        onClick={() => {
          if (selectedBrand) setSelectedBrand(null)
          else setSelectedCategory(null)
        }}>{selectedBrand ? '← Volver a las marcas' : '← Volver a las categorías'}</button>}
'@
$oldTitle = @'
        <h1 ref={headingRef} tabIndex={-1}>{selectedBrand ? `Productos de ${selectedBrand.nombre}`
          : selectedCategory ? `Marcas de ${selectedCategory.nombre}` : 'Categorías disponibles'}</h1>
'@
$newTitle = @'
        <div className="public-catalog-title-row">
          {selectedCategory && <button type="button" className="public-catalog-back-icon"
            aria-label={selectedBrand ? 'Volver a las marcas' : 'Volver a las categorías'}
            title={selectedBrand ? 'Volver a las marcas' : 'Volver a las categorías'}
            onClick={() => {
              if (selectedBrand) setSelectedBrand(null)
              else setSelectedCategory(null)
            }}>
            <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor"
              strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
              <path d="m12 19-7-7 7-7M5 12h14" />
            </svg>
          </button>}
          <h1 ref={headingRef} tabIndex={-1}>{selectedBrand ? `Productos de ${selectedBrand.nombre}`
            : selectedCategory ? `Marcas de ${selectedCategory.nombre}` : 'Categorías disponibles'}</h1>
        </div>
'@
if (-not $page.Contains($oldButton) -or -not $page.Contains($oldTitle)) { throw 'La navegación cambió; revisar antes de editar.' }
$page = $page.Replace($oldButton, '').Replace($oldTitle, $newTitle)
$page = $page.Replace("selectedBrand ? 'public-catalog-heading public-catalog-heading-sticky'", "selectedCategory ? 'public-catalog-heading public-catalog-heading-sticky'")
$styles = @'

.public-catalog-title-row { display: flex; align-items: center; gap: 10px; }
.public-catalog-title-row h1 { flex: 1; min-width: 0; overflow-wrap: anywhere; }
.public-catalog-back-icon {
  display: grid;
  place-items: center;
  flex-shrink: 0;
  width: 44px;
  height: 44px;
  padding: 8px;
  border: 0;
  border-radius: 50%;
  background: transparent;
  color: #245f4e;
  cursor: pointer;
}
.public-catalog-back-icon:hover { background: #eaf0ed; }
'@
$utf8 = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($pagePath, $page, $utf8)
[System.IO.File]::AppendAllText($cssPath, $styles, $utf8)
