$root = 'C:\Users\coy81\Desktop\SISTEMA\SISTEMA\Front-End.web\Front-End'
$path = Join-Path $root 'src/features/auth/session/AuthProvider.tsx'
$text = Get-Content $path -Raw
$text = "import { requestJson } from '../../../shared/api/httpClient'`n" + $text
$text = $text.Replace('useCallback, useEffect, useState', 'useCallback, useEffect, useRef, useState')
$text = $text.Replace("  const [notice, setNotice] = useState('')", "  const [notice, setNotice] = useState('')`n  const currentSession = useRef<StaffSession | null>(null)")
$text = $text.Replace("  const logout = useCallback((message = '') => {", @'
  const logout = useCallback((message = '') => {
    const previous = currentSession.current
    currentSession.current = null
    if (!message && previous && previous.expiresAt > Date.now()) {
      void requestJson('/Login/cerrar-sesion', { method: 'POST', token: previous.token }).catch(() => {
        if (!currentSession.current)
          setNotice('Cerraste la sesión en este dispositivo, pero no se pudo confirmar el registro del cierre en el servidor.')
      })
    }
'@)
$text = $text.Replace('    setSession(next)', '    currentSession.current = next' + "`n" + '    setSession(next)')
Set-Content $path $text -Encoding utf8
