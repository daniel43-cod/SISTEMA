$ErrorActionPreference = 'Stop'
$frontendRoot = 'C:\Users\coy81\Desktop\SISTEMA\SISTEMA\Front-End.web\Front-End'
$manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'public-products-files.json') -Raw | ConvertFrom-Json
$utf8 = New-Object System.Text.UTF8Encoding($false)
$brandsPath = Join-Path $frontendRoot 'src/features/public-catalog/components/PublicBrandsList.tsx'
$brands = [System.IO.File]::ReadAllText($brandsPath)
$brands = "import type { PublicBrand } from '../types/brand'`n" + $brands
$brands = $brands.Replace('({ idCategoria }: { idCategoria: number })', '({ idCategoria, onSelect }: { idCategoria: number; onSelect: (brand: PublicBrand) => void })')
$brands = $brands.Replace('<CatalogImage key={brand.urlImagen}', '<button type="button" className="public-category-select" onClick={() => onSelect(brand)}><CatalogImage key={brand.urlImagen}')
$brands = $brands.Replace('<h2>{brand.nombre}</h2>', '<h2>{brand.nombre}</h2></button>')
if (-not $brands.Contains('onClick={() => onSelect(brand)}')) { throw 'Revisar tarjetas de marcas.' }
$imagePath = Join-Path $frontendRoot 'src/features/public-catalog/components/CatalogImage.tsx'
$image = [System.IO.File]::ReadAllText($imagePath)
$image = $image.Replace("kind: 'categorias' | 'marcas'", "kind: 'categorias' | 'marcas' | 'productos'")
$image = $image.Replace(': /^\/uploads\/marcas\/[a-zA-Z0-9.-]+$/', ": kind === 'marcas'`n      ? /^\/uploads\/marcas\/[a-zA-Z0-9.-]+$/`n      : /^\/uploads\/productos\/[a-zA-Z0-9.-]+$/")
if (-not $image.Contains('uploads\/productos')) { throw 'Revisar imágenes de productos.' }
$cssPath = Join-Path $frontendRoot 'src/features/public-catalog/pages/PublicCategoriesPage.css'
$css = [System.IO.File]::ReadAllText($cssPath) + $manifest.css
foreach ($entry in $manifest.files.PSObject.Properties) {
    $target = Join-Path $frontendRoot $entry.Name
    if ((Test-Path -LiteralPath $target) -and $entry.Name -ne 'src/features/public-catalog/pages/PublicCategoriesPage.tsx') { throw "El archivo ya existe: $target" }
}
foreach ($entry in $manifest.files.PSObject.Properties) {
    $target = Join-Path $frontendRoot $entry.Name
    [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($target)) | Out-Null
    [System.IO.File]::WriteAllText($target, [string]$entry.Value, $utf8)
}
[System.IO.File]::WriteAllText($brandsPath, $brands, $utf8)
[System.IO.File]::WriteAllText($imagePath, $image, $utf8)
[System.IO.File]::WriteAllText($cssPath, $css, $utf8)
Write-Output 'Productos, presentaciones y carrito público integrados.'
