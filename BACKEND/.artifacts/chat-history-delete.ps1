$ErrorActionPreference = 'Stop'
$frontendRoot = 'C:\Users\coy81\Desktop\SISTEMA\SISTEMA\Front-End.web\Front-End'
$chatPath = Join-Path $frontendRoot 'src/features/customer-chat/components/CustomerChat.tsx'
$apiPath = Join-Path $frontendRoot 'src/features/customer-chat/api/customerChatApi.ts'
$cssPath = Join-Path $frontendRoot 'src/features/public-catalog/pages/PublicCategoriesPage.css'
$chat = [System.IO.File]::ReadAllText($chatPath).Replace("`r`n", "`n")
$api = [System.IO.File]::ReadAllText($apiPath)
$oldList = @'
    {showHistory && chats.length > 0 && <nav className="customer-chat-list" aria-label="Tus conversaciones">
      {chats.map(chat => <button key={chat.idConversacion} type="button" disabled={sending}
        aria-current={selected === chat.idConversacion ? 'true' : undefined}
        onClick={() => { setSelected(chat.idConversacion); setNotice(''); setShowHistory(false) }}>{chat.contenido}</button>)}
      {hasMore && <button type="button" disabled={sending || loading} onClick={() => setPage(v => v + 1)}>Más chats</button>}
    </nav>}
'@
$newList = @'
    {showHistory && <nav id="customer-chat-history" className="customer-chat-list customer-chat-history" aria-label="Historial de conversaciones">
      {!loading && !chats.length && <p>No tenés conversaciones guardadas.</p>}
      {chats.map(chat => <div key={chat.idConversacion} className="customer-chat-history-row">
        <button type="button" className="customer-chat-history-title" disabled={sending || deleting !== null}
          aria-current={selected === chat.idConversacion ? 'true' : undefined}
          onClick={() => { setSelected(chat.idConversacion); setNotice(''); setShowHistory(false) }}>
          {chat.contenido}
        </button>
        <button type="button" className="customer-chat-delete" title="Borrar chat"
          aria-label={`Borrar chat: ${chat.contenido}`} disabled={sending || deleting !== null}
          onClick={() => void removeChat(chat)}>
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor"
            strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d="M3 6h18M9 6V3h6v3M5 6l1 15h12l1-15M10 10v7M14 10v7" />
          </svg>
        </button>
      </div>)}
      {deleting !== null && <p role="status">Borrando conversación…</p>}
      {hasMore && <button type="button" disabled={sending || loading || deleting !== null} onClick={() => setPage(v => v + 1)}>Más chats</button>}
    </nav>}
'@
if (-not $chat.Contains($oldList)) { throw 'El historial cambió; revisar antes de editar.' }
$chat = $chat.Replace('ChatSendError, listChats, listMessages, sendChat', 'ChatSendError, deleteChat, listChats, listMessages, sendChat')
$chat = $chat.Replace('  const [showHistory, setShowHistory] = useState(false)', "  const [showHistory, setShowHistory] = useState(false)`n  const [deleting, setDeleting] = useState<number | null>(null)`n  const deletingRef = useRef(false)")
$deleteHandler = @'
  async function removeChat(chat: Chat) {
    if (sending || deletingRef.current) return
    if (!window.confirm('¿Borrar esta conversación y todos sus mensajes? No podrás recuperarlos.')) return
    deletingRef.current = true; setDeleting(chat.idConversacion); setError('')
    try {
      await deleteChat(token, chat.idConversacion)
      setChats(current => current.filter(row => row.idConversacion !== chat.idConversacion))
      if (selected === chat.idConversacion) setSelected(null)
      setNotice('Conversación eliminada.'); setPage(1); setRevision(v => v + 1)
    } catch (failure) {
      if (failure instanceof ApiError && (failure.status === 401 || failure.status === 403)) onUnauthorized()
      else setError(failure instanceof Error ? failure.message : 'No se pudo borrar el chat.')
    } finally { deletingRef.current = false; setDeleting(null) }
  }
'@
$chat = $chat.Replace("  return <div className=`"customer-chat`">", $deleteHandler + "`n  return <div className=`"customer-chat`">")
$chat = $chat.Replace($oldList, $newList)
$chat = $chat.Replace('disabled={sending}', 'disabled={sending || deleting !== null}')
# Los reemplazos anteriores también pueden tocar ChatThread: allí no existe deleting.
$threadEnd = $chat.IndexOf('export function CustomerChat')
$thread = $chat.Substring(0, $threadEnd).Replace('disabled={sending || deleting !== null}', 'disabled={sending}')
$chat = $thread + $chat.Substring($threadEnd)
$chat = $chat.Replace('disabled={sending || loading} onClick={() => { setPage(1)', 'disabled={sending || loading || deleting !== null} onClick={() => { setPage(1)')
$chat = $chat.Replace('title="Ver chats" aria-label="Ver chats"', 'title="Historial" aria-label="Abrir historial" aria-controls="customer-chat-history"')
$chat = $chat.Replace('{ready && <ChatThread', '{ready && <div className="customer-chat-thread-container" inert={deleting !== null}><ChatThread')
$chat = $chat.Replace('refreshVersion={messageRevision} />}', 'refreshVersion={messageRevision} /></div>}')
$api += @'

export async function deleteChat(token: string, conversationId: number) {
  if (!Number.isSafeInteger(conversationId) || conversationId <= 0) throw new ApiError('La conversación no es válida.', 400)
  try { await requestJson(`/Conversaciones/${conversationId}`, { method: 'DELETE', token }) }
  catch (failure) {
    if (failure instanceof ApiError && failure.status === 404) return
    throw failure
  }
}
'@
$css = @'

.public-assistant-sidebar .customer-chat-history {
  display: flex; flex-direction: column; flex-shrink: 0; gap: 4px;
  max-height: 220px; overflow-x: hidden; overflow-y: auto;
  padding: 8px; margin: 8px 0; border: 1px solid #e1e7ed; border-radius: 10px; background: #f8fafc;
}
.customer-chat-history-row { display: flex; align-items: center; gap: 4px; min-width: 0; }
.customer-chat-history button.customer-chat-history-title { flex: 1; min-width: 0; max-width: none; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; border: 0; background: transparent; }
.customer-chat-history button.customer-chat-history-title[aria-current="true"] { background: #eaf0ed; }
.customer-chat-history button.customer-chat-delete { display: grid; place-items: center; flex: 0 0 36px; min-width: 36px; max-width: 36px; min-height: 36px; padding: 6px; border: 0; background: transparent; color: #a33333; }
.customer-chat-history button.customer-chat-delete:hover { background: #fee2e2; }
.customer-chat-thread-container { display: flex; flex: 1; min-height: 0; flex-direction: column; }
'@
$utf8 = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($chatPath, $chat, $utf8)
[System.IO.File]::WriteAllText($apiPath, $api, $utf8)
[System.IO.File]::AppendAllText($cssPath, $css, $utf8)
