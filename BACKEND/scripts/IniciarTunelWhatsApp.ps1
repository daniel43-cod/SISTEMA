param([int]$Puerto = 5273)
$ErrorActionPreference = 'Stop'
if ($Puerto -ne 5273) { throw 'Usa el puerto dedicado 5273 del perfil webhook-local.' }
$comando = Get-Command cloudflared -ErrorAction SilentlyContinue
$ejecutable = if ($comando) { $comando.Source } else { $null }
if (-not $ejecutable) {
    foreach ($base in @($env:ProgramFiles, ${env:ProgramFiles(x86)})) {
        if ($base) {
            $candidato = Join-Path $base 'cloudflared\cloudflared.exe'
            if (Test-Path -LiteralPath $candidato) { $ejecutable = $candidato; break }
        }
    }
}
if (-not $ejecutable) { throw 'No se encontró cloudflared. Instala Cloudflare.cloudflared o abre una terminal nueva.' }
function EstadoHttp([string]$Ruta, [string]$Metodo = 'GET') {
    try { return [int](Invoke-WebRequest -Uri ('http://localhost:5273' + $Ruta) -Method $Metodo -UseBasicParsing).StatusCode }
    catch {
        if ($_.Exception.Response) { return [int]$_.Exception.Response.StatusCode }
        throw 'No se puede conectar a la API. Inicia dotnet run --project API_SISTEMA --launch-profile webhook-local.'
    }
}
if ((EstadoHttp '/swagger/index.html') -ne 404) { throw 'El puerto no está restringido al webhook. No se abrirá el túnel.' }
if ((EstadoHttp '/api/whatsapp/webhook') -ne 403 -or (EstadoHttp '/api/whatsapp/webhook' 'POST') -ne 401) {
    throw 'Configura WhatsApp:VerifyToken, AppSecret, BusinessAccountId y PhoneNumberId antes de abrir el túnel.'
}
Write-Host 'El túnel solo apunta al puerto de webhook. Copia la URL https://...trycloudflare.com y agrega /api/whatsapp/webhook en Meta.'
Write-Host 'Mantén esta terminal abierta. Ctrl+C cierra el túnel; una ejecución nueva cambia la URL.'
& $ejecutable tunnel --url http://localhost:5273
