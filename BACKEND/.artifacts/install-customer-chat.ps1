$ErrorActionPreference = 'Stop'
$frontendRoot = 'C:\Users\coy81\Desktop\SISTEMA\SISTEMA\Front-End.web\Front-End'
$manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'customer-chat-files.json') -Raw | ConvertFrom-Json
$panelPath = Join-Path $frontendRoot 'src/features/public-catalog/components/FloatingPublicAssistant.tsx'
$cssPath = Join-Path $frontendRoot 'src/features/public-catalog/pages/PublicCategoriesPage.css'
$panel = [System.IO.File]::ReadAllText($panelPath)
if (-not $panel.Contains('<strong>{session.correoElectronico}</strong>')) { throw 'Revisar panel del cliente.' }
foreach ($entry in $manifest.PSObject.Properties) {
    if (Test-Path -LiteralPath (Join-Path $frontendRoot $entry.Name)) { throw "Ya existe $($entry.Name)" }
}
$panel = "import { CustomerChat } from '../../customer-chat/components/CustomerChat'`n" + $panel
$panel = $panel.Replace('className="public-assistant-dialog"', 'className={session ? "public-assistant-dialog public-assistant-dialog-chat" : "public-assistant-dialog"}')
$panel = $panel.Replace('<button type="button" onClick={logout}>Cerrar sesión</button>', '<button type="button" onClick={logout}>Cerrar sesión</button><CustomerChat key={session.idCuentaCliente} token={session.token} onUnauthorized={logout} />')
$css = @'

.public-assistant-dialog-chat { width: min(820px, calc(100% - 24px)); }
.customer-chat { margin-top: 18px; }
.customer-chat-toolbar { display: flex; flex-wrap: wrap; gap: 8px; }
.customer-chat button { min-height: 40px; font: inherit; padding: 8px 12px; border: 1px solid #cbd5e1; border-radius: 8px; background: white; color: #245f4e; margin-top: 0; }
.customer-chat button:disabled { opacity: .5; cursor: not-allowed; }
.customer-chat-list { display: flex; gap: 8px; overflow-x: auto; padding: 12px 0; }
.customer-chat-list button { min-width: 130px; max-width: 200px; flex-shrink: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; text-align: left; }
.customer-chat-list button[aria-current="true"] { background: #eaf0ed; border-color: #245f4e; }
.customer-chat-messages { height: min(42dvh, 380px); min-height: 180px; overflow-y: auto; overscroll-behavior: contain; padding: 12px; margin-top: 12px; background: #f5f7fa; border-radius: 12px; }
.customer-chat-turn { display: grid; gap: 10px; margin-bottom: 14px; }
.customer-chat-bubble { padding: 12px; border-radius: 12px; max-width: 90%; overflow-wrap: anywhere; white-space: pre-wrap; }
.customer-chat-user { background: #dcf0e6; margin-left: auto; }
.customer-chat-ai { background: white; margin-right: auto; }
.customer-chat-bubble strong { margin-top: 0; font-size: 12px; }
.public-assistant-intro .customer-chat-bubble p { margin-top: 6px; color: #182b3a; font-size: 14px; }
.customer-chat-compose { display: grid; grid-template-columns: 1fr auto; gap: 8px; margin-top: 12px; }
.customer-chat-compose label { grid-column: 1 / -1; font-size: 14px; font-weight: 600; }
.customer-chat-compose textarea { min-width: 0; resize: vertical; max-height: 180px; padding: 10px; border: 1px solid #94a3b8; border-radius: 8px; font: inherit; }
.customer-chat-compose button { background: #1d4ed8; color: white; align-self: end; }
.customer-chat-refresh { margin-top: 10px !important; }
@media (max-width: 520px) {
  .public-assistant-dialog-chat .public-assistant-intro { padding: 48px 12px 16px; }
  .customer-chat-bubble { max-width: 96%; }
}
'@
$utf8 = New-Object System.Text.UTF8Encoding($false)
foreach ($entry in $manifest.PSObject.Properties) {
    $target = Join-Path $frontendRoot $entry.Name
    [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($target)) | Out-Null
    [System.IO.File]::WriteAllText($target, [string]$entry.Value, $utf8)
}
[System.IO.File]::WriteAllText($panelPath, $panel, $utf8)
[System.IO.File]::AppendAllText($cssPath, $css, $utf8)
