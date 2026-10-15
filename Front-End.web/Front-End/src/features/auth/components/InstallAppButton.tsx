import { useEffect, useState } from 'react'

interface InstallPromptEvent extends Event {
  prompt(): Promise<void>
  userChoice: Promise<{ outcome: 'accepted' | 'dismissed' }>
}

export function InstallAppButton() {
  const [prompt, setPrompt] = useState<InstallPromptEvent | null>(null)
  const [installed, setInstalled] = useState(false)
  const [showHelp, setShowHelp] = useState(false)

  useEffect(() => {
    const standalone = window.matchMedia('(display-mode: standalone)')
    const update = () => setInstalled(standalone.matches ||
      Boolean((navigator as Navigator & { standalone?: boolean }).standalone))
    const capture = (event: Event) => {
      event.preventDefault()
      setPrompt(event as InstallPromptEvent)
      setShowHelp(false)
    }
    const onInstalled = () => {
      setInstalled(true)
      setPrompt(null)
      setShowHelp(false)
    }
    update()
    standalone.addEventListener('change', update)
    window.addEventListener('beforeinstallprompt', capture)
    window.addEventListener('appinstalled', onInstalled)
    return () => {
      standalone.removeEventListener('change', update)
      window.removeEventListener('beforeinstallprompt', capture)
      window.removeEventListener('appinstalled', onInstalled)
    }
  }, [])

  async function install() {
    if (!prompt) {
      setShowHelp(value => !value)
      return
    }
    try {
      await prompt.prompt()
      await prompt.userChoice
    } catch {
      setShowHelp(true)
    } finally {
      setPrompt(null)
    }
  }

  if (installed) return null

  return <div className="login-install">
    <button type="button" className="login-install-button" onClick={install}
      aria-describedby={showHelp ? 'install-help' : undefined}>
      <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor"
        strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
        <path d="M12 3v12m-5-5 5 5 5-5M4 16v5h16v-5" />
      </svg>
      Instalar aplicación
    </button>
    {showHelp && <p id="install-help" role="status">
      {window.isSecureContext
        ? 'En Android, abre el menú del navegador y busca “Instalar aplicación” o “Agregar a pantalla principal”. En iPhone, abre Safari, toca Compartir y luego “Agregar a pantalla de inicio”. Si la opción no aparece, la instalación aún no está disponible en esta página.'
        : 'Para instalar la aplicación, abre el sitio con una conexión HTTPS segura. Desde una IP local con HTTP la instalación no está disponible.'}
    </p>}
  </div>
}
