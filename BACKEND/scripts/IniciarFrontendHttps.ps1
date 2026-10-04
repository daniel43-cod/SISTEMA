param([string]$LanIp = '192.168.0.3')
$ErrorActionPreference = 'Stop'
$workspace = Split-Path $PSScriptRoot -Parent
$frontend = Join-Path (Split-Path $workspace -Parent) 'Front-End.web/Front-End'
$certFolder = Join-Path $workspace '.certs'
foreach ($file in @('dev-cert.pem', 'dev-key.pem', 'rootCA.pem')) {
    if (-not (Test-Path -LiteralPath (Join-Path $certFolder $file))) {
        throw 'Primero ejecuta scripts/PrepararHttps.ps1.'
    }
}
# Node necesita esta autoridad al iniciar para validar el backend HTTPS.
$env:NODE_EXTRA_CA_CERTS = Join-Path $certFolder 'rootCA.pem'
$env:DEV_HTTPS_CERT = Join-Path $certFolder 'dev-cert.pem'
$env:DEV_HTTPS_KEY = Join-Path $certFolder 'dev-key.pem'
$env:DEV_LAN_IP = $LanIp
$env:API_PROXY_TARGET = 'https://localhost:7272'
Push-Location $frontend
try { npm run dev } finally { Pop-Location }
