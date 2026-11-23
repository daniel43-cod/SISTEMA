import { createPortal } from 'react-dom'
import { useEffect, useRef, useState } from 'react'

import { ApiError } from '../../../shared/api/ApiError'
import { loadTicketPos } from '../api/ticketPosApi'
import type { TicketPos } from '../api/ticketPosApi'
import { ticketPosHtml } from '../utils/ticketPosHtml'

import './TicketPosDialog.css'

export function TicketPosDialog({saleId,token,onClose,onUnauthorized}:{saleId:number;token:string;onClose:()=>void;onUnauthorized:()=>void}) {
  const dialog=useRef<HTMLDialogElement>(null)
  const frame=useRef<HTMLIFrameElement>(null)
  const width = 80 as const
  const [ticket,setTicket]=useState<TicketPos|null>(null)
  const [html,setHtml]=useState('')
  const [ready,setReady]=useState(false)
  const [error,setError]=useState('')
  const [revision,setRevision]=useState(0)
  useEffect(()=>{const previous=document.activeElement;const panel=dialog.current;panel?.showModal();return()=>{panel?.close();if(previous instanceof HTMLElement&&previous.isConnected)previous.focus()}},[])
  useEffect(()=>{const c=new AbortController();setTicket(null);setHtml('');setReady(false);setError('');void loadTicketPos(saleId,token,c.signal).then(t=>{if(!c.signal.aborted)setTicket(t)}).catch(e=>{if(c.signal.aborted)return;if(e instanceof ApiError&&e.status===401)onUnauthorized();else setError(e instanceof Error?e.message:'No se pudo preparar el ticket.')});return()=>c.abort()},[saleId,token,revision,onUnauthorized])
  useEffect(()=>{let active=true;setReady(false);setHtml('');if(!ticket)return;void (async()=>{const qr=ticket.certificacion?await (await import('qrcode')).default.toDataURL(ticket.certificacion.qrUrl,{errorCorrectionLevel:'M',margin:4,width:384}):undefined;if(active)setHtml(ticketPosHtml(ticket,width,qr))})().catch(()=>{if(active)setError('No se pudo generar el ticket y su QR. No se envió nada a la impresora.')});return()=>{active=false}},[ticket,width])
  function print(){if(!ready||!frame.current?.contentWindow)return;try{frame.current.contentWindow.focus();frame.current.contentWindow.print()}catch{setError('El navegador no pudo abrir la impresión. Prueba desde una PC con la POS de red instalada.')}}
  return createPortal(<dialog ref={dialog} className="ticket-pos-dialog" aria-labelledby="ticket-pos-title" onCancel={e=>{e.preventDefault();onClose()}}>
    <header><h2 id="ticket-pos-title">Imprimir ticket</h2><button type="button" onClick={onClose} aria-label="Cerrar impresión">×</button></header>



    {error?<div role="alert"><p>{error}</p><button type="button" onClick={()=>setRevision(v=>v+1)}>Reintentar carga</button></div>:html?<iframe key={html} ref={frame} title="Vista previa del ticket POS" sandbox="allow-same-origin allow-modals" srcDoc={html} style={{width:width+'mm'}} onLoad={()=>setReady(true)}/>:<p role="status">Preparando ticket…</p>}
    <p id="ticket-pos-network-note">Para imprimir en la POS, conecta este dispositivo a la misma red de la impresora. Desde fuera de esa red no podrás imprimir en ella con esta opción.</p>
    <footer><button type="button" onClick={onClose}>Cerrar</button><button type="button" disabled={!ready||!!error} aria-describedby="ticket-pos-network-note" onClick={print}>Imprimir en POS de red</button></footer>

  </dialog>,document.body)
}
