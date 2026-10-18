$ErrorActionPreference = 'Stop'
$root = 'C:\Users\coy81\Desktop\SISTEMA\SISTEMA\Front-End.web\Front-End'
$files = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'lookup-files.json') -Raw | ConvertFrom-Json
foreach ($file in $files) {
  $target = Join-Path $root $file.path
  [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($target)) | Out-Null
  [System.IO.File]::WriteAllText($target, $file.content, [System.Text.UTF8Encoding]::new($false))
}
foreach ($relative in @('src/features/categories/api/categoriesApi.ts', 'src/features/brands/api/brandsApi.ts')) {
  $target = Join-Path $root $relative
  $content = [System.IO.File]::ReadAllText($target)
  $content = "import { clearCatalogLookupCache } from '../../../shared/cache/catalogLookupCache.ts'`n" + $content
  if ($relative.Contains('categories')) {
    $content = $content.Replace('    try { const result = parseCategory(response);', "    clearCatalogLookupCache()`n    try { const result = parseCategory(response);")
    $content = $content.Replace('  try {' + "`r`n" + '    const updated = parseCategory(response)', "  clearCatalogLookupCache()`r`n  try {`r`n    const updated = parseCategory(response)")
  } else {
    $content = $content.Replace('    const row = result as Partial<BrandResponse> | null', "    clearCatalogLookupCache()`n    const row = result as Partial<BrandResponse> | null")
    $content = $content.Replace('  const row = response as Partial<BrandResponse> | null', "  clearCatalogLookupCache()`n  const row = response as Partial<BrandResponse> | null")
  }
  [System.IO.File]::WriteAllText($target, $content, [System.Text.UTF8Encoding]::new($false))
}
$target = Join-Path $root 'src/features/auth/session/AuthProvider.tsx'
$content = [System.IO.File]::ReadAllText($target)
$content = "import { clearCatalogLookupCache } from '../../../shared/cache/catalogLookupCache'`n" + $content
$content = $content.Replace('    currentSession.current = null', "    clearCatalogLookupCache()`n    currentSession.current = null")
$content = $content.Replace("    setNotice('')", "    clearCatalogLookupCache()`n    setNotice('')")
[System.IO.File]::WriteAllText($target, $content, [System.Text.UTF8Encoding]::new($false))
