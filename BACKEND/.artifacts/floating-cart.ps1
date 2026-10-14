$ErrorActionPreference = 'Stop'
$frontendRoot = 'C:\Users\coy81\Desktop\SISTEMA\SISTEMA\Front-End.web\Front-End'
$pagePath = Join-Path $frontendRoot 'src/features/public-catalog/pages/PublicCategoriesPage.tsx'
$hookPath = Join-Path $frontendRoot 'src/features/public-catalog/hooks/usePublicCart.ts'
$cssPath = Join-Path $frontendRoot 'src/features/public-catalog/pages/PublicCategoriesPage.css'
$componentPath = Join-Path $frontendRoot 'src/features/public-catalog/components/FloatingPublicCart.tsx'
$page = [System.IO.File]::ReadAllText($pagePath)
$hook = [System.IO.File]::ReadAllText($hookPath)
if (-not $page.Contains('{cart.items.length > 0 && <PublicCart')) { throw 'Revisar integración actual del carrito.' }
if (Test-Path -LiteralPath $componentPath) { throw 'Ya existe el carrito flotante.' }
$page = $page.Replace("import { PublicCart } from '../components/PublicCart'", "import { FloatingPublicCart } from '../components/FloatingPublicCart'")
$page = $page.Replace('{cart.items.length > 0 && <PublicCart', '<FloatingPublicCart')
$page = $page.Replace('setQuantity={cart.setQuantity} clear={cart.clear} />}', 'setQuantity={cart.setQuantity} clear={cart.clear} />')
$hook = $hook.Replace('localStorage.getItem', 'sessionStorage.getItem').Replace('localStorage.setItem', 'sessionStorage.setItem')
$hook = $hook.Replace('El carrito funciona en esta página, pero no pudo guardarse para tu próxima visita.', 'No se pudo guardar el carrito en esta pestaña. Si recargas la página, podrías perderlo.')
$styles = @'

.public-cart-floating {
  position: fixed;
  left: max(18px, env(safe-area-inset-left));
  bottom: max(18px, env(safe-area-inset-bottom));
  z-index: 20;
  display: grid;
  place-items: center;
  width: 60px;
  height: 60px;
  border: 0;
  border-radius: 50%;
  background: #245f4e;
  color: white;
  box-shadow: 0 6px 20px #182b3a35;
}
.public-cart-floating:hover { background: #1b493c; }
.public-cart-badge { position: absolute; top: -3px; right: -4px; min-width: 25px; padding: 4px 6px; border-radius: 20px; background: #b63636; color: white; font-size: 12px; font-weight: 700; }
.public-cart-dialog { width: min(680px, calc(100% - 24px)); max-height: 85dvh; padding: 0; border: 0; border-radius: 18px; background: white; color: #182b3a; }
.public-cart-dialog::backdrop { background: #10202db3; }
.public-cart-dialog-content { position: relative; }
.public-cart-dialog .public-cart { margin: 0; padding-top: 54px; border: 0; }
.public-cart-close { position: absolute; top: 8px; right: 10px; display: grid; place-items: center; width: 40px; height: 40px; border: 0; border-radius: 50%; background: #f0f6f3; color: #245f4e; }
.public-cart-announcement { position: absolute; width: 1px; height: 1px; padding: 0; margin: -1px; overflow: hidden; clip-path: inset(50%); white-space: nowrap; }
.public-catalog-footer { padding-bottom: 96px; }
'@
$utf8 = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($pagePath, $page, $utf8)
[System.IO.File]::WriteAllText($hookPath, $hook, $utf8)
[System.IO.File]::WriteAllText($componentPath, [System.IO.File]::ReadAllText((Join-Path $PSScriptRoot 'FloatingPublicCart.tsx')), $utf8)
[System.IO.File]::AppendAllText($cssPath, $styles, $utf8)
