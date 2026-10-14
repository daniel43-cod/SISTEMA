param([string]$DiagnosticsPath)
$ErrorActionPreference = 'Stop'
$renameMap = @{ id_producto='IdProducto'; codigo_barra='CodigoBarra'; nombre='Nombre'; stock='Stock'; stock_minimo='StockMinimo'; imagen='Imagen'; costo_unitario='CostoUnitario'; impuesto='Impuesto'; fecha_creacion='FechaCreacion' }
$changes = @{}
foreach ($diagnostic in Get-Content -LiteralPath $DiagnosticsPath) {
  if ($diagnostic -match '^.*?(C:\\.*\.cs)\((\d+),(\d+)\): error CS(1061|0117): .Productos.') {
    $sourcePath = $Matches[1]; $lineIndex = [int]$Matches[2] - 1; $columnIndex = [int]$Matches[3] - 1
    if (-not $sourcePath.StartsWith('C:\Users\coy81\Desktop\SISTEMA\SISTEMA\BACKEND\')) { throw 'Ruta fuera del backend.' }
    if (-not $changes.ContainsKey($sourcePath)) { $changes[$sourcePath] = @{} }
    $changes[$sourcePath]["$lineIndex,$columnIndex"] = [PSCustomObject]@{ Line = $lineIndex; Column = $columnIndex }
  }
}
$updated = 0
foreach ($sourcePath in $changes.Keys) {
  $content = [System.IO.File]::ReadAllText($sourcePath)
  $lineEnding = if ($content.Contains("`r`n")) { "`r`n" } else { "`n" }
  $lines = $content.Replace("`r`n", "`n").Split("`n")
  foreach ($position in ($changes[$sourcePath].Values | Sort-Object Line, Column -Descending)) {
    $lineIndex = $position.Line; $columnIndex = $position.Column
    $line = $lines[$lineIndex]
    $token = [regex]::Match($line.Substring($columnIndex), '^[A-Za-z_][A-Za-z_0-9]*').Value
    if (-not $renameMap.ContainsKey($token)) { throw "Token inesperado: $token en $sourcePath" }
    $lines[$lineIndex] = $line.Substring(0, $columnIndex) + $renameMap[$token] + $line.Substring($columnIndex + $token.Length)
    $updated++
  }
  [System.IO.File]::WriteAllText($sourcePath, ($lines -join $lineEnding), (New-Object System.Text.UTF8Encoding($false)))
}
Write-Output "$updated referencias actualizadas."
