$ErrorActionPreference = 'Stop'
$frontendRoot = 'C:\Users\coy81\Desktop\SISTEMA\SISTEMA\Front-End.web\Front-End'
$componentPath = Join-Path $frontendRoot 'src/features/public-catalog/components/PublicProductsList.tsx'
$cssPath = Join-Path $frontendRoot 'src/features/public-catalog/pages/PublicCategoriesPage.css'
$component = [System.IO.File]::ReadAllText($componentPath)
$before = '<div><strong>{presentation.nombre}</strong><span>{money.format(presentation.precio)}</span>'
$after = @'
<div className="public-presentation-info">
                <div className="public-presentation-heading">
                  <strong>{presentation.nombre}</strong>
                  <span>{money.format(presentation.precio)}</span>
                </div>
'@
if (-not $component.Contains($before)) { throw 'La estructura de la presentación cambió; revisar antes de editar.' }
$styles = @'

.public-presentation-info { flex: 1; min-width: 0; }
.public-presentation-heading { display: flex; align-items: baseline; justify-content: space-between; gap: 6px; }
.public-presentation-heading strong { min-width: 0; }
.public-presentation .public-presentation-heading span { margin-top: 0; flex-shrink: 0; white-space: nowrap; }
'@
$utf8 = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($componentPath, $component.Replace($before, $after), $utf8)
[System.IO.File]::AppendAllText($cssPath, $styles, $utf8)
