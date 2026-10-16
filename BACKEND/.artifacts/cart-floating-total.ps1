$ErrorActionPreference = 'Stop'
$frontendRoot = 'C:\Users\coy81\Desktop\SISTEMA\SISTEMA\Front-End.web\Front-End'
$componentPath = Join-Path $frontendRoot 'src/features/public-catalog/components/FloatingPublicCart.tsx'
$cssPath = Join-Path $frontendRoot 'src/features/public-catalog/pages/PublicCategoriesPage.css'
$component = [System.IO.File]::ReadAllText($componentPath)
$before = '<span className="public-cart-badge" aria-hidden="true">{props.count > 99 ? ''99+'' : props.count}</span>'
if (-not $component.Contains($before)) { throw 'Revisar estructura del botón del carrito.' }
$after = @'
<span className="public-cart-badge" aria-hidden="true">{props.count > 99 ? '99+' : props.count}</span>
      <span className="public-cart-floating-total" aria-hidden="true">
        <small>Total estimado</small>
        <strong>{new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' }).format(props.total)}</strong>
      </span>
'@
$component = $component.Replace($before, $after)
$component = $component.Replace('Abrir carrito: ${props.count} artículos', 'Abrir carrito: ${props.count} artículos, total estimado ${props.total.toFixed(2)} quetzales')
$component = $component.Replace('{props.count} artículos en el carrito.', '{props.count} artículos en el carrito. Total estimado: {props.total.toFixed(2)} quetzales.')
$styles = @'

.public-cart-floating-total { position: absolute; top: calc(100% + 6px); right: 0; display: grid; gap: 2px; padding: 5px 8px; border: 1px solid #dce3e9; border-radius: 8px; background: white; color: #245f4e; box-shadow: 0 2px 8px #182b3a12; white-space: nowrap; }
.public-cart-floating-total small { font-size: 10px; font-weight: 400; }
.public-cart-floating-total strong { font-size: 13px; font-variant-numeric: tabular-nums; }
@media (min-width: 901px) {
  .public-catalog { --public-header-height: 118px; }
}
@media (max-width: 900px) {
  .public-cart-floating { bottom: calc(max(18px, env(safe-area-inset-bottom)) + 42px); }
  .public-assistant-floating { bottom: calc(max(18px, env(safe-area-inset-bottom)) + 118px); }
}
'@
$utf8 = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($componentPath, $component, $utf8)
[System.IO.File]::AppendAllText($cssPath, $styles, $utf8)
