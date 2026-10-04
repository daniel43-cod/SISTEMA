import { existsSync, readFileSync } from 'node:fs'
import { fileURLToPath } from 'node:url'
import react from '@vitejs/plugin-react'
import { defineConfig, loadEnv } from 'vite'
import { VitePWA } from 'vite-plugin-pwa'

// https://vite.dev/config/
export default defineConfig(({ mode, command }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const value = (name: string) => process.env[name] || env[name]
  const localCert = fileURLToPath(new URL('../../BACKEND/.certs/dev-cert.pem', import.meta.url))
  const localKey = fileURLToPath(new URL('../../BACKEND/.certs/dev-key.pem', import.meta.url))
  const cert = value('DEV_HTTPS_CERT') || (existsSync(localCert) ? localCert : undefined)
  const key = value('DEV_HTTPS_KEY') || (existsSync(localKey) ? localKey : undefined)
  if (command === 'serve' && Boolean(cert) !== Boolean(key))
    throw new Error('Configura ambos certificados: DEV_HTTPS_CERT y DEV_HTTPS_KEY.')
  const https = command === 'serve' && cert && key
    ? { cert: readFileSync(cert), key: readFileSync(key) }
    : undefined
  const target = value('API_PROXY_TARGET') || (https ? 'https://localhost:7272' : 'http://localhost:5272')
  if (!/^https?:\/\//i.test(target)) throw new Error('API_PROXY_TARGET debe usar HTTP o HTTPS.')
  return ({
  server: {
    host: '0.0.0.0',
    port: 5173,
    strictPort: true,
    https,
    proxy: {
      '/api': { target, changeOrigin: true, secure: true },
      '/uploads': { target, changeOrigin: true, secure: true },
    },
  },
  plugins: [
    react(),
    VitePWA({
      strategies: 'generateSW',
      injectRegister: 'script',
      // Las nuevas versiones se activan al cerrar todas las pestañas de la app.
      registerType: 'prompt',
      pwaAssets: {
        preset: 'minimal-2023',
        image: 'public/icono_principal.jpeg',
      },
      manifest: {
        id: '/',
        name: 'Sistema',
        short_name: 'Sistema',
        description: 'Aplicación web del sistema',
        lang: 'es',
        start_url: '/',
        scope: '/',
        display: 'standalone',
        background_color: '#ffffff',
        theme_color: '#ffffff',
      },
      workbox: {
        globPatterns: ['**/*.{js,css,html,ico,png,svg,jpg,jpeg,webp,woff,woff2}'],
        cleanupOutdatedCaches: true,
        navigateFallback: 'index.html',
        navigateFallbackDenylist: [/^\/api(?:\/|$)/i, /^\/uploads(?:\/|$)/i],
        // Solo recursos del frontend; las respuestas del backend no se guardan.
        runtimeCaching: [],
      },
      devOptions: { enabled: false },
    }),
  ],
})
})
