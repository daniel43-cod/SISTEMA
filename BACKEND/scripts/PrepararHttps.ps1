param([string]$LanIp = '192.168.0.3')
$ErrorActionPreference = 'Stop'
$workspace = Split-Path $PSScriptRoot -Parent
$certFolder = Join-Path $workspace '.certs'
if (-not (Get-Command mkcert -ErrorAction SilentlyContinue)) {
    throw 'Instala mkcert desde https://github.com/FiloSottile/mkcert antes de ejecutar este script.'
}
# Este comando registra la autoridad local de desarrollo como confiable.
& mkcert -install
if ($LASTEXITCODE -ne 0) { throw 'No se pudo instalar la autoridad local.' }
[IO.Directory]::CreateDirectory($certFolder) | Out-Null
& mkcert -cert-file (Join-Path $certFolder 'dev-cert.pem') -key-file (Join-Path $certFolder 'dev-key.pem') localhost 127.0.0.1 ::1 $LanIp
if ($LASTEXITCODE -ne 0) { throw 'No se pudo generar el certificado.' }
$authorityFolder = & mkcert -CAROOT
if ($LASTEXITCODE -ne 0) { throw 'No se pudo localizar la autoridad.' }
# Copiar solo el certificado público: nunca rootCA-key.pem.
Copy-Item -LiteralPath (Join-Path $authorityFolder 'rootCA.pem') -Destination (Join-Path $certFolder 'rootCA.pem')
Write-Output "Certificados preparados en $certFolder. Instala solo rootCA.pem en el Android de prueba."
