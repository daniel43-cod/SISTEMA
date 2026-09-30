# Sistema — React + TypeScript + Vite + PWA

## Desarrollo y PWA

Ejecuta `npm install` y `npm run dev` para desarrollar. El service worker está
desactivado en desarrollo para evitar que la caché oculte cambios.

Para probar la versión PWA localmente:

```sh
npm run build
npm run preview
```

Abre la dirección de localhost que muestra Vite. En DevTools → Application,
verifica el manifiesto y el service worker. Después de la primera carga y de
que el worker esté activo, activa Offline en DevTools y recarga para comprobar
que carga la interfaz. La instalación se ofrece en navegadores compatibles.

`vite.config.ts` contiene el nombre provisional `Sistema`, los colores y la
configuración PWA. El plugin genera `dist/manifest.webmanifest`, `dist/sw.js`,
su registro y los iconos de instalación desde `public/icono_principal.jpeg`
en cada compilación. No edites los archivos generados en `dist`.
El icono público es una copia del original de `src/icons`; reemplaza la copia
de `public` cuando quieras cambiar el icono instalado.

Solo se precargan recursos estáticos del frontend. Las peticiones al backend
siguen necesitando conexión; no se guardan respuestas de API ni operaciones
pendientes. Las rutas `/api` se excluyen del fallback de navegación.

Las actualizaciones se descargan en segundo plano y se activan cuando se
cierran todas las pestañas/ventanas de la aplicación y se vuelve a abrir.
No se fuerza una recarga durante la edición de formularios.

Para producción, publica `dist` mediante HTTPS en la raíz del dominio y
configura el servidor para devolver `index.html` en las rutas del frontend.
Sirve `sw.js` y el manifiesto sin caché permanente (por ejemplo,
`Cache-Control: no-cache`) para que se detecten nuevas versiones.

Referencia: https://vite-pwa-org.netlify.app/guide/

## Plantilla original

This template provides a minimal setup to get React working in Vite with HMR and some Oxlint rules.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Oxc](https://oxc.rs)
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/)

## React Compiler

The React Compiler is not enabled on this template because of its impact on dev & build performances. To add it, see [this documentation](https://react.dev/learn/react-compiler/installation).

## Expanding the Oxlint configuration

If you are developing a production application, we recommend enabling type-aware lint rules by installing `oxlint-tsgolint` and editing `.oxlintrc.json`:

```json
{
  "$schema": "./node_modules/oxlint/configuration_schema.json",
  "plugins": ["react", "typescript", "oxc"],
  "options": {
    "typeAware": true
  },
  "rules": {
    "react/rules-of-hooks": "error",
    "react/only-export-components": ["warn", { "allowConstantExport": true }]
  }
}
```

See the [Oxlint rules documentation](https://oxc.rs/docs/guide/usage/linter/rules) for the full list of rules and categories.
