$ErrorActionPreference = 'Stop'
$root = 'C:\Users\coy81\Desktop\SISTEMA\SISTEMA\Front-End.web\Front-End'
$target = Join-Path $root 'src/features/inventory/components/InventoryLookup.tsx'
$content = [System.IO.File]::ReadAllText($target)
$content = $content.Replace('label, items, selected, loading, error, onSelect', 'label, items, selected, loading, error, onSelect, query, onQueryChange')
$content = $content.Replace('  onSelect: (id?: number) => void', "  query: string; onQueryChange: (value: string) => void`n  onSelect: (id?: number) => void")
$content = $content.Replace("  const [query, setQuery] = useState('')", '')
$content = $content.Replace('setQuery(', 'onQueryChange(')
[System.IO.File]::WriteAllText($target, $content, [System.Text.UTF8Encoding]::new($false))
$target = Join-Path $root 'src/features/inventory/pages/InventoryPage.tsx'
$content = [System.IO.File]::ReadAllText($target)
$content = $content.Replace('  const [categoryId, setCategoryId]', "  const [categoryQuery, setCategoryQuery] = useState('')`n  const [brandQuery, setBrandQuery] = useState('')`n  const [categoryId, setCategoryId]")
$content = $content.Replace('  if (!allowed) return null', @'
  function activateSearch(mode: 'product' | 'category' | 'brand') {
    setPage(1)
    if (mode !== 'product') { setQuery(''); setText(''); setSuggestionsOpen(false) }
    if (mode !== 'category') { setCategoryId(undefined); setCategoryQuery('') }
    if (mode !== 'brand') { setBrandId(undefined); setBrandQuery('') }
  }

  if (!allowed) return null
'@)
$content = $content.Replace('setText(query.trim()); setPage(1);', "activateSearch('product'); setText(query.trim()); setPage(1);")
$content = $content.Replace('            setQuery(value);', "            activateSearch('product'); setQuery(value);")
$content = $content.Replace('              setQuery(item.nombre);', "              activateSearch('product'); setQuery(item.nombre);")
$content = $content.Replace('        selected={categoryId}', @'
        query={categoryQuery} onQueryChange={value => { activateSearch('category'); setCategoryQuery(value) }}
        selected={categoryId}
'@)
$content = $content.Replace('        selected={brandId}', @'
        query={brandQuery} onQueryChange={value => { activateSearch('brand'); setBrandQuery(value) }}
        selected={brandId}
'@)
[System.IO.File]::WriteAllText($target, $content, [System.Text.UTF8Encoding]::new($false))
