$ErrorActionPreference = 'Stop'
$frontendRoot = 'C:\Users\coy81\Desktop\SISTEMA\SISTEMA\Front-End.web\Front-End'
$manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'purchases-front-files.json') -Raw | ConvertFrom-Json
$layoutPath = Join-Path $frontendRoot 'src/app/layouts/StaffLayout.tsx'
$layout = [System.IO.File]::ReadAllText($layoutPath)
$before = ": current.id === 'caja' ? <CashPage key={session.user.id} /> :"
if (-not $layout.Contains($before)) { throw 'Revisar navegación antes de editar.' }
foreach ($entry in $manifest.PSObject.Properties) {
    if (Test-Path -LiteralPath (Join-Path $frontendRoot $entry.Name)) { throw "Ya existe $($entry.Name)" }
}
$utf8 = New-Object System.Text.UTF8Encoding($false)
foreach ($entry in $manifest.PSObject.Properties) {
    $target = Join-Path $frontendRoot $entry.Name
    [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($target)) | Out-Null
    [System.IO.File]::WriteAllText($target, [string]$entry.Value, $utf8)
}
$layout = "import { PurchasesPage } from '../../features/purchases/pages/PurchasesPage'`n" + $layout
$layout = $layout.Replace($before, ": current.id === 'caja' ? <CashPage key={session.user.id} /> : current.id === 'compras' ? <PurchasesPage key={session.user.id} /> :")
[System.IO.File]::WriteAllText($layoutPath, $layout, $utf8)
