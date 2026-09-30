import type { StaffSection } from './navigationItems'

const paths: Record<StaffSection, string> = {
  inicio: 'M3 10 12 3l9 7M5 9v12h5v-7h4v7h5V9',
  ventas: 'M3 3h2l3 12h10l3-9H6M9 20h.01M18 20h.01',
  caja: 'M4 7h16v14H4zM7 3h10v4M8 11h8M8 16h2M14 16h2',
  pedidos: 'm3 7 9-4 9 4v10l-9 4-9-4V7Zm0 0 9 4 9-4M12 11v10M7 5l10 4',
  productos: 'M4 4h6v6H4zM14 4h6v6h-6zM4 14h6v6H4zM14 14h6v6h-6z',
  compras: 'M5 7h14l1 14H4L5 7ZM9 7V5a3 3 0 0 1 6 0v2',
  gastos: 'M5 3h14v18l-3-2-4 2-4-2-3 2V3ZM8 8h8M8 12h8M8 16h4',
  reportes: 'M4 3v18h17M8 16v-5M13 16V7M18 16V4',
  configuracion: 'M4 7h16M4 17h16M8 4v6M16 14v6',
}
export function NavigationIcon({ section }: { section: StaffSection }) {
  return <svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor"
    strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
    <path d={paths[section]} />
  </svg>
}