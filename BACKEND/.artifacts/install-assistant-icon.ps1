$ErrorActionPreference = 'Stop'
$frontendRoot = 'C:\Users\coy81\Desktop\SISTEMA\SISTEMA\Front-End.web\Front-End'
$componentPath = Join-Path $frontendRoot 'src/features/public-catalog/components/FloatingPublicAssistant.tsx'
$pagePath = Join-Path $frontendRoot 'src/features/public-catalog/pages/PublicCategoriesPage.tsx'
$cssPath = Join-Path $frontendRoot 'src/features/public-catalog/pages/PublicCategoriesPage.css'
if (Test-Path -LiteralPath $componentPath) { throw 'El componente del asistente ya existe.' }
$page = [System.IO.File]::ReadAllText($pagePath)
if (-not $page.Contains('<FloatingPublicCart items=')) { throw 'Revisar ubicación de los botones flotantes.' }
$page = "import { FloatingPublicAssistant } from '../components/FloatingPublicAssistant'`n" + $page
$page = $page.Replace('<FloatingPublicCart items=', "<FloatingPublicAssistant />`n      <FloatingPublicCart items=")
$css = @'

.public-assistant-floating {
  position: fixed;
  right: max(18px, env(safe-area-inset-right));
  bottom: calc(max(18px, env(safe-area-inset-bottom)) + 76px);
  z-index: 20;
  display: grid;
  place-items: center;
  width: 60px;
  height: 60px;
  border: 0;
  border-radius: 50%;
  background: #1d4ed8;
  color: white;
  box-shadow: 0 6px 20px #182b3a35;
}
.public-assistant-floating:hover { background: #1e40af; }
.public-assistant-dialog { width: min(420px, calc(100% - 24px)); max-height: 85dvh; padding: 0; border: 0; border-radius: 18px; color: #182b3a; background: white; }
.public-assistant-dialog::backdrop { background: #10202db3; }
.public-assistant-intro { position: relative; padding: 56px 24px 24px; }
.public-assistant-intro h2 { font-size: 24px; margin-bottom: 16px; }
.public-assistant-intro p { margin-top: 12px; line-height: 1.6; color: #475569; }
.public-assistant-intro .public-cart-close { font-size: 26px; }
'@
$utf8 = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($componentPath, [System.IO.File]::ReadAllText((Join-Path $PSScriptRoot 'FloatingPublicAssistant.tsx')), $utf8)
[System.IO.File]::WriteAllText($pagePath, $page, $utf8)
[System.IO.File]::AppendAllText($cssPath, $css, $utf8)
