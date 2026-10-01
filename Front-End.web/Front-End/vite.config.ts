import react from '@vitejs/plugin-react'
import { defineConfig, loadEnv } from 'vite'
import { VitePWA } from 'vite-plugin-pwa'

// https://vite.dev/config/
export default defineConfig(({ mode }) => ({
  server: {
    proxy: {
      '/uploads/categorias': { target: loadEnv(mode, process.cwd(), '').API_PROXY_TARGET || 'http://localhost:5272', changeOrigin: true },
      '/api': { target: loadEnv(mode, process.cwd(), '').API_PROXY_TARGET || 'http://localhost:5272', changeOrigin: true },
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
}))
