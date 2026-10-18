$ErrorActionPreference = 'Stop'
$root = 'C:\Users\coy81\Desktop\SISTEMA\SISTEMA\Front-End.web\Front-End'
$files = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'inventory-files.json') -Raw -Encoding UTF8 | ConvertFrom-Json
foreach ($file in $files) {
  $target = Join-Path $root $file.path
  [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($target)) | Out-Null
  [System.IO.File]::WriteAllText($target, $file.content, [System.Text.UTF8Encoding]::new($false))
}
