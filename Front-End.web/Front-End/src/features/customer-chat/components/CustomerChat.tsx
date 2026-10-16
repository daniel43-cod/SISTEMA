import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { ApiError } from '../../../shared/api/ApiError'
import { ChatSendError, listChats, listMessages, sendChat } from '../api/customerChatApi'
import type { Chat, ChatMessage } from '../api/customerChatApi'

function ChatThread({ token, conversationId, onConversation, onBusy, onUnauthorized, refreshVersion }: {
  token: string; conversationId: number | null; onConversation: (id: number, notice?: string) => void
  onBusy: (busy: boolean) => void; onUnauthorized: () => void; refreshVersion: number
}) {
  const [messages, setMessages] = useState<ChatMessage[]>([])
  const [loading, setLoading] = useState(conversationId !== null)
  const [hasOlder, setHasOlder] = useState(false)
  const [sending, setSending] = useState(false)
  const [draft, setDraft] = useState('')
  const [error, setError] = useState('')
  const [blocked, setBlocked] = useState(false)
  const [revision, setRevision] = useState(0)
  const busy = useRef(false)
  const mounted = useRef(true)
  const end = useRef<HTMLDivElement>(null)
  useEffect(() => { mounted.current = true; return () => { mounted.current = false } }, [])
  useEffect(() => {
    if (conversationId === null) return
    const controller = new AbortController()
    async function load() {
      setLoading(true); setError('')
      try {
        const rows = await listMessages(token, conversationId!, undefined, controller.signal)
        if (!controller.signal.aborted) { setMessages(rows); setHasOlder(rows.length === 50); setBlocked(false) }
      } catch (failure) {
        if (controller.signal.aborted) return
        if (failure instanceof ApiError && (failure.status === 401 || failure.status === 403)) onUnauthorized()
        else { setError(failure instanceof Error ? failure.message : 'No se pudieron cargar los mensajes.'); setBlocked(true) }
      } finally { if (!controller.signal.aborted) setLoading(false) }
    }
    void load()
    return () => controller.abort()
  }, [token, conversationId, revision, refreshVersion, onUnauthorized])
  useEffect(() => { end.current?.scrollIntoView({ block: 'nearest' }) }, [messages.length, sending])
  async function older() {
    if (busy.current || !conversationId || !messages.length) return
    busy.current = true; setLoading(true); setError('')
    try {
      const rows = await listMessages(token, conversationId, messages[0].idMensaje)
      if (mounted.current) { setMessages(current => [...rows, ...current]); setHasOlder(rows.length === 50) }
    } catch (failure) {
      if (failure instanceof ApiError && (failure.status === 401 || failure.status === 403)) onUnauthorized()
      else if (mounted.current) setError(failure instanceof Error ? failure.message : 'No se pudo cargar más mensajes.')
    } finally { busy.current = false; if (mounted.current) setLoading(false) }
  }
  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (busy.current || blocked || loading || !draft.trim() || draft.length > 2000) return
    const question = draft.trim()
    busy.current = true; setSending(true); setError(''); onBusy(true)
    try {
      const reply = await sendChat(token, question, conversationId)
      if (!mounted.current) return
      setMessages(current => [...current, { idMensaje: reply.idMensaje, mensaje: question, respuesta: reply.respuesta }])
      setDraft('')
      onConversation(reply.idConversacion)
    } catch (failure) {
      if (!mounted.current) return
      if (failure instanceof ApiError && (failure.status === 401 || failure.status === 403)) onUnauthorized()
      else if (failure instanceof ChatSendError && failure.conversationId) {
        setDraft('')
        onConversation(failure.conversationId, failure.message)
        if (failure.conversationId === conversationId) setRevision(v => v + 1)
      } else {
        setError(failure instanceof Error ? failure.message : 'No se pudo enviar el mensaje.')
        setBlocked(!(failure instanceof ApiError) || failure.status === 0 || failure.status >= 500)
      }
    } finally {
      busy.current = false
      if (mounted.current) { setSending(false); onBusy(false) }
    }
  }
  return <div className="customer-chat-thread">
    <div className="customer-chat-messages" aria-label="Mensajes de la conversación">
      {hasOlder && <button type="button" disabled={loading || sending} onClick={() => void older()}>Ver mensajes anteriores</button>}
      {loading && <p role="status">Cargando mensajes…</p>}
      {!loading && !messages.length && !error && <p>¿En qué podemos ayudarte? Escribí tu consulta sobre nuestros productos.</p>}
      {messages.map(message => <div key={message.idMensaje} className="customer-chat-turn">
        <div className="customer-chat-bubble customer-chat-user"><strong>Vos</strong><p>{message.mensaje}</p></div>
        <div className="customer-chat-bubble customer-chat-ai"><strong>Asistente</strong>
          <p>{message.respuesta ?? 'Esta pregunta todavía no tiene una respuesta guardada.'}</p></div>
      </div>)}
      {sending && <div className="customer-chat-bubble customer-chat-user"><strong></strong><p>{draft.trim()}</p></div>}
      {sending && <p role="status">Escribiendo...</p>}
      <div ref={end} />
    </div>
    {error && <p className="customer-login-error" role="alert">{error}</p>}

    <form className="customer-chat-compose" onSubmit={submit} aria-busy={sending}>
      <label htmlFor="customer-chat-input">Tu mensaje</label>
      <textarea id="customer-chat-input" rows={2} maxLength={2000} value={draft}
        placeholder="Escribí tu consulta…" disabled={sending} onChange={event => setDraft(event.target.value)} />
      <button type="submit" disabled={sending || loading || blocked || !draft.trim()}>{sending ? 'Enviando…' : 'Enviar'}</button>
    </form>
  </div>
}

export function CustomerChat({ token, onUnauthorized, email }: { token: string; onUnauthorized: () => void; email: string }) {
  const [chats, setChats] = useState<Chat[]>([])
  const [selected, setSelected] = useState<number | null>(null)
  const [page, setPage] = useState(1)
  const [hasMore, setHasMore] = useState(false)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [revision, setRevision] = useState(0)
  const [sending, setSending] = useState(false)
  const initial = useRef(true)
  const [ready, setReady] = useState(false)
  const [showHistory, setShowHistory] = useState(false)
  const [messageRevision, setMessageRevision] = useState(0)
  useEffect(() => {
    const controller = new AbortController()
    async function load() {
      setLoading(true); setError('')
      try {
        const rows = await listChats(token, page, controller.signal)
        if (controller.signal.aborted) return
        setChats(current => page === 1 ? rows : [...current, ...rows.filter(row => !current.some(c => c.idConversacion === row.idConversacion))])
        setHasMore(rows.length === 25)
        if (initial.current) { setSelected(rows[0]?.idConversacion ?? null); initial.current = false; setReady(true) }
      } catch (failure) {
        if (controller.signal.aborted) return
        if (failure instanceof ApiError && (failure.status === 401 || failure.status === 403)) onUnauthorized()
        else setError(failure instanceof Error ? failure.message : 'No se pudieron cargar los chats.')
      } finally { if (!controller.signal.aborted) setLoading(false) }
    }
    void load()
    return () => controller.abort()
  }, [token, page, revision, onUnauthorized])
  function conversation(id: number, message = '') {
    if (id > 0) setSelected(id)
    setNotice(message); setPage(1); setRevision(v => v + 1)
  }
  return <div className="customer-chat">
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
    {loading && <p role="status">Cargando chats…</p>}
    {error && <p role="alert" className="customer-login-error">{error}</p>}
    {showHistory && chats.length > 0 && <nav className="customer-chat-list" aria-label="Tus conversaciones">
      {chats.map(chat => <button key={chat.idConversacion} type="button" disabled={sending}
        aria-current={selected === chat.idConversacion ? 'true' : undefined}
        onClick={() => { setSelected(chat.idConversacion); setNotice(''); setShowHistory(false) }}>{chat.contenido}</button>)}
      {hasMore && <button type="button" disabled={sending || loading} onClick={() => setPage(v => v + 1)}>Más chats</button>}
    </nav>}
    {notice && <p role="status">{notice}</p>}
    {ready && <ChatThread key={selected ?? 'new'} token={token} conversationId={selected}
      onConversation={conversation} onBusy={setSending} onUnauthorized={onUnauthorized} refreshVersion={messageRevision} />}
  </div>
}
