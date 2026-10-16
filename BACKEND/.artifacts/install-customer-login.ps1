$ErrorActionPreference = 'Stop'
$frontendRoot = 'C:\Users\coy81\Desktop\SISTEMA\SISTEMA\Front-End.web\Front-End'
$manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'customer-login-files.json') -Raw | ConvertFrom-Json
$panelPath = Join-Path $frontendRoot 'src/features/public-catalog/components/FloatingPublicAssistant.tsx'
$cssPath = Join-Path $frontendRoot 'src/features/public-catalog/pages/PublicCategoriesPage.css'
$panel = [System.IO.File]::ReadAllText($panelPath)
$before = '<p>Para conversar con el asistente necesitarás iniciar sesión o crear una cuenta de cliente.</p>'
if (-not $panel.Contains($before)) { throw 'El panel cambió; revisar antes de editar.' }
foreach ($entry in $manifest.PSObject.Properties) {
    if (Test-Path -LiteralPath (Join-Path $frontendRoot $entry.Name)) { throw "Ya existe $($entry.Name)" }
}
$panel = "import { CustomerLoginForm } from '../../customer-auth/components/CustomerLoginForm'`nimport { useCustomerSession } from '../../customer-auth/hooks/useCustomerSession'`n" + $panel
$panel = $panel.Replace('  const [open, setOpen] = useState(false)', "  const [open, setOpen] = useState(false)`n  const { session, login, logout, notice } = useCustomerSession()")
$after = @'
{session ? <div className="customer-session">
          <p role="status">Sesión iniciada</p>
          <strong>{session.correoElectronico}</strong>
          {notice && <p role="status">{notice}</p>}
          <button type="button" onClick={logout}>Cerrar sesión</button>
        </div> : <CustomerLoginForm key={String(open)} onLogin={login} />}
'@
$panel = $panel.Replace($before, $after)
$css = @'

.customer-login-form { margin-top: 20px; }
.customer-login-form fieldset { display: grid; gap: 10px; border: 0; padding: 0; margin: 16px 0 0; min-width: 0; }
.customer-login-form label { font-size: 14px; font-weight: 600; }
.customer-login-form input { width: 100%; min-height: 44px; padding: 10px 12px; font: inherit; border: 1px solid #94a3b8; border-radius: 8px; background: white; color: #182b3a; }
.customer-login-form button, .customer-session button { min-height: 44px; padding: 10px 16px; margin-top: 10px; border: 0; border-radius: 8px; background: #1d4ed8; color: white; font: inherit; }
.customer-login-form button:disabled { opacity: .6; cursor: wait; }
.public-assistant-intro p.customer-login-error { color: #991b1b; padding: 12px; border-radius: 8px; background: #fef2f2; }
.customer-session { margin-top: 20px; }
.customer-session strong { display: block; margin-top: 8px; overflow-wrap: anywhere; }
'@
$utf8 = New-Object System.Text.UTF8Encoding($false)
foreach ($entry in $manifest.PSObject.Properties) {
    $target = Join-Path $frontendRoot $entry.Name
    [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($target)) | Out-Null
    [System.IO.File]::WriteAllText($target, [string]$entry.Value, $utf8)
}
[System.IO.File]::WriteAllText($panelPath, $panel, $utf8)
[System.IO.File]::AppendAllText($cssPath, $css, $utf8)
