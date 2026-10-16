$ErrorActionPreference = 'Stop'
$frontendRoot = 'C:\Users\coy81\Desktop\SISTEMA\SISTEMA\Front-End.web\Front-End'
$pagePath = Join-Path $frontendRoot 'src/features/public-catalog/pages/PublicCategoriesPage.tsx'
$panelPath = Join-Path $frontendRoot 'src/features/public-catalog/components/FloatingPublicAssistant.tsx'
$cssPath = Join-Path $frontendRoot 'src/features/public-catalog/pages/PublicCategoriesPage.css'
$page = [System.IO.File]::ReadAllText($pagePath)
if (-not $page.Contains('<FloatingPublicAssistant />') -or -not $page.Contains('return <div className="public-catalog">')) { throw 'Revisar estructura del catálogo antes de integrar.' }
$page = $page.Replace('  const cart = usePublicCart()', "  const cart = usePublicCart()`n  const [assistantOpen, setAssistantOpen] = useState(false)")
$page = $page.Replace('return <div className="public-catalog">', 'return <div className={assistantOpen ? "public-catalog public-catalog-with-assistant" : "public-catalog"}>')
$page = $page.Replace('<FloatingPublicAssistant />', '<FloatingPublicAssistant open={assistantOpen} onOpenChange={setAssistantOpen} />')
$css = @'

/* El asistente ocupa su propio espacio, sin bloquear el catálogo en escritorio. */
.public-catalog { --assistant-panel-width: 360px; }
.public-assistant-floating[hidden], .public-assistant-sidebar[hidden] { display: none; }
.public-assistant-sidebar {
  position: fixed;
  inset: 0 0 0 auto;
  z-index: 40;
  display: flex;
  flex-direction: column;
  width: var(--assistant-panel-width);
  height: 100dvh;
  background: #fff;
  border-left: 1px solid #dce3e9;
  box-shadow: -4px 0 18px #182b3a0a;
  color: #182b3a;
}
.public-assistant-sidebar-header { display: flex; align-items: center; justify-content: space-between; gap: 12px; flex-shrink: 0; padding: 14px 16px; border-bottom: 1px solid #e1e7ed; }
.public-assistant-sidebar-header h2 { margin: 0; font-size: 18px; }
.public-assistant-close { display: grid; place-items: center; width: 38px; height: 38px; padding: 0; border: 0; border-radius: 8px; background: transparent; color: #475569; font-size: 26px; }
.public-assistant-close:hover { background: #f0f4f7; }
.public-assistant-sidebar-body { display: flex; flex-direction: column; flex: 1; min-height: 0; padding: 12px; overflow: auto; overscroll-behavior: contain; }
.public-assistant-sidebar .public-assistant-intro { padding: 4px; }
.public-assistant-account { display: flex; flex-shrink: 0; align-items: center; gap: 8px; padding-bottom: 10px; border-bottom: 1px solid #e1e7ed; }
.public-assistant-account span { flex: 1; min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; font-size: 13px; color: #64748b; }
.public-assistant-account button { min-height: 36px; padding: 6px 10px; border: 0; border-radius: 8px; background: #f1f5f9; color: #475569; font: inherit; }
.public-assistant-sidebar .customer-chat { display: flex; flex-direction: column; flex: 1; min-height: 0; margin-top: 12px; }
.public-assistant-sidebar .customer-chat-toolbar { flex-shrink: 0; }
.public-assistant-sidebar .customer-chat-toolbar button { flex: 1; padding: 7px; font-size: 12px; }
.public-assistant-sidebar .customer-chat-list { flex-shrink: 0; max-height: 96px; padding: 10px 0; }
.public-assistant-sidebar .customer-chat-thread { display: flex; flex-direction: column; flex: 1; min-height: 0; }
.public-assistant-sidebar .customer-chat-messages { flex: 1; height: auto; min-height: 140px; padding: 10px; }
.public-assistant-sidebar .customer-chat-bubble p { margin: 6px 0 0; font-size: 14px; line-height: 1.55; }
.public-assistant-sidebar .customer-chat-bubble strong { display: block; font-size: 12px; }
.public-assistant-sidebar .customer-chat-compose { flex-shrink: 0; padding-bottom: env(safe-area-inset-bottom); }
.public-assistant-sidebar .customer-chat-compose textarea { max-height: 120px; }
.public-assistant-sidebar .customer-chat-compose button { padding: 8px 10px; }
.public-assistant-sidebar .customer-chat-refresh { flex-shrink: 0; align-self: flex-start; font-size: 12px; }
.public-assistant-sidebar .customer-login-error { margin: 8px 0; padding: 10px; color: #991b1b; background: #fef2f2; border-radius: 8px; font-size: 13px; }
@media (min-width: 901px) {
  .public-catalog-with-assistant { margin-right: var(--assistant-panel-width); }
  .public-catalog-with-assistant .public-cart-floating { right: calc(var(--assistant-panel-width) + 18px); }
}
@media (max-width: 900px) {
  .public-assistant-sidebar { width: min(100%, 400px); }
  .public-catalog-with-assistant .public-cart-floating { display: none; }
}
@media (max-width: 520px) {
  .public-assistant-sidebar { width: 100%; }
}
'@
$utf8 = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($panelPath, [System.IO.File]::ReadAllText((Join-Path $PSScriptRoot 'AssistantSidebar.tsx')), $utf8)
[System.IO.File]::WriteAllText($pagePath, $page, $utf8)
[System.IO.File]::AppendAllText($cssPath, $css, $utf8)
