$ErrorActionPreference = 'Stop'
$frontendRoot = 'C:\Users\coy81\Desktop\SISTEMA\SISTEMA\Front-End.web\Front-End'
$fileManifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'public-catalog-files.json') -Raw | ConvertFrom-Json
$utf8 = New-Object System.Text.UTF8Encoding($false)
foreach ($entry in $fileManifest.PSObject.Properties) {
    $target = Join-Path $frontendRoot $entry.Name
    if (Test-Path -LiteralPath $target) { throw "El archivo ya existe: $target" }
}
$appPath = Join-Path $frontendRoot 'src/app/App.tsx'
$appContent = [System.IO.File]::ReadAllText($appPath)
$oldReturn = 'return <AppProviders><AppContent /></AppProviders>'
if (-not $appContent.Contains($oldReturn)) { throw 'App.tsx cambió; revisar antes de integrar.' }
foreach ($entry in $fileManifest.PSObject.Properties) {
    $target = Join-Path $frontendRoot $entry.Name
    [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($target)) | Out-Null
    [System.IO.File]::WriteAllText($target, [string]$entry.Value, $utf8)
}
$appContent = "import { PublicCategoriesPage } from '../features/public-catalog/pages/PublicCategoriesPage'`n" + $appContent
$newReturn = @'
const catalogPath = `${import.meta.env.BASE_URL}catalogo`
  const currentPath = window.location.pathname.replace(/\/+$/, '')
  if (currentPath === catalogPath.replace(/\/+$/, '')) return <PublicCategoriesPage />
  return <AppProviders><AppContent /></AppProviders>
'@
$appContent = $appContent.Replace($oldReturn, $newReturn)
[System.IO.File]::WriteAllText($appPath, $appContent, $utf8)
Write-Output 'Página pública creada e integrada en /catalogo.'
