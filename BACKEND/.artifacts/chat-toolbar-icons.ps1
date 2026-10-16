$ErrorActionPreference = 'Stop'
$frontendRoot = 'C:\Users\coy81\Desktop\SISTEMA\SISTEMA\Front-End.web\Front-End'
$chatPath = Join-Path $frontendRoot 'src/features/customer-chat/components/CustomerChat.tsx'
$panelPath = Join-Path $frontendRoot 'src/features/public-catalog/components/FloatingPublicAssistant.tsx'
$cssPath = Join-Path $frontendRoot 'src/features/public-catalog/pages/PublicCategoriesPage.css'
$chat = [System.IO.File]::ReadAllText($chatPath).Replace("`r`n", "`n")
$panel = [System.IO.File]::ReadAllText($panelPath).Replace("`r`n", "`n")
$oldToolbar = @'
    <div className="customer-chat-toolbar">
      <button type="button" disabled={sending} onClick={() => { setSelected(null); setNotice('') }}>+ Nuevo chat</button>
      <button type="button" disabled={sending || loading} onClick={() => { setPage(1); setRevision(v => v + 1) }}>Actualizar chats</button>
    </div>
'@
$newToolbar = @'
    <div className="public-assistant-account">
      <span title={email}>{email}</span>
      <button type="button" className="customer-chat-icon" title="Nuevo chat" aria-label="Nuevo chat"
        disabled={sending} onClick={() => { setSelected(null); setNotice(''); setShowHistory(false) }}>
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"
          strokeLinecap="round" aria-hidden="true"><path d="M12 5v14M5 12h14" /></svg>
      </button>
      <button type="button" className="customer-chat-icon" title="Actualizar chats y mensajes"
        aria-label="Actualizar chats y mensajes" disabled={sending || loading}
        onClick={() => { setPage(1); setRevision(v => v + 1); setMessageRevision(v => v + 1) }}>
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8"
          strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
          <path d="M20 7v5h-5M4 17v-5h5M6.1 7a7 7 0 0 1 11.5-1L20 9M4 15l2.4 3A7 7 0 0 0 18 17" />
        </svg>
      </button>
      <button type="button" className="customer-chat-icon" title="Ver chats" aria-label="Ver chats"
        aria-expanded={showHistory} disabled={sending} onClick={() => setShowHistory(value => !value)}>
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8"
          strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
          <path d="M4 4h16v12H8l-4 4V4Z" /><path d="M8 8h8M8 12h5" />
        </svg>
      </button>
      <button type="button" disabled={sending} onClick={onUnauthorized}>Salir</button>
    </div>
'@
$oldRefresh = @'
    <button type="button" className="customer-chat-refresh" disabled={sending || loading}
      onClick={() => { if (conversationId) setRevision(v => v + 1); else onConversation(0, 'Actualizá la lista de chats para comprobar si tu mensaje quedó guardado.') }}>
      Actualizar mensajes
    </button>
'@
$oldAccount = @'
          <div className="public-assistant-account">
            <span title={session.correoElectronico}>{session.correoElectronico}</span>
            <button type="button" onClick={logout}>Salir</button>
          </div>
'@
if (-not $chat.Contains($oldToolbar) -or -not $chat.Contains($oldRefresh) -or -not $panel.Contains($oldAccount)) { throw 'La estructura cambió; revisar antes de editar.' }
$chat = $chat.Replace('function ChatThread({ token, conversationId, onConversation, onBusy, onUnauthorized }', 'function ChatThread({ token, conversationId, onConversation, onBusy, onUnauthorized, refreshVersion }')
$chat = $chat.Replace('  onBusy: (busy: boolean) => void; onUnauthorized: () => void', '  onBusy: (busy: boolean) => void; onUnauthorized: () => void; refreshVersion: number')
$chat = $chat.Replace('[token, conversationId, revision, onUnauthorized]', '[token, conversationId, revision, refreshVersion, onUnauthorized]')
$chat = $chat.Replace($oldRefresh, '').Replace($oldToolbar, $newToolbar)
$chat = $chat.Replace('export function CustomerChat({ token, onUnauthorized }: { token: string; onUnauthorized: () => void })', 'export function CustomerChat({ token, onUnauthorized, email }: { token: string; onUnauthorized: () => void; email: string })')
$chat = $chat.Replace('  const [ready, setReady] = useState(false)', "  const [ready, setReady] = useState(false)`n  const [showHistory, setShowHistory] = useState(false)`n  const [messageRevision, setMessageRevision] = useState(0)")
$chat = $chat.Replace('{chats.length > 0 && <nav', '{showHistory && chats.length > 0 && <nav')
$chat = $chat.Replace("setSelected(chat.idConversacion); setNotice('')", "setSelected(chat.idConversacion); setNotice(''); setShowHistory(false)")
$chat = $chat.Replace('onConversation={conversation} onBusy={setSending} onUnauthorized={onUnauthorized} />', 'onConversation={conversation} onBusy={setSending} onUnauthorized={onUnauthorized} refreshVersion={messageRevision} />')
$panel = $panel.Replace($oldAccount, '').Replace('token={session.token} onUnauthorized={logout}', 'token={session.token} onUnauthorized={logout} email={session.correoElectronico}')
$css = @'

.public-assistant-sidebar .customer-chat > .public-assistant-account { margin-bottom: 10px; gap: 4px; }
.public-assistant-sidebar .customer-chat button.customer-chat-icon { display: grid; place-items: center; flex-shrink: 0; width: 32px; min-height: 36px; padding: 4px; border: 0; background: transparent; color: #475569; }
.public-assistant-sidebar .customer-chat button.customer-chat-icon:hover { background: #f1f5f9; }
.public-assistant-sidebar .customer-chat > .public-assistant-account > span { font-size: 12px; }
.public-assistant-sidebar .customer-chat > .public-assistant-account > button:last-child { padding: 6px; font-size: 12px; }
'@
$utf8 = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($chatPath, $chat, $utf8)
[System.IO.File]::WriteAllText($panelPath, $panel, $utf8)
[System.IO.File]::AppendAllText($cssPath, $css, $utf8)
