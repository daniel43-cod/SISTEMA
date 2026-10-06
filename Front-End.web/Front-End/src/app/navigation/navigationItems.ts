import type { StaffRole } from '../../features/auth/types/auth'

export type StaffSection = 'inicio' | 'ventas' | 'caja' | 'pedidos' | 'productos' | 'compras' | 'gastos' | 'reportes' | 'configuracion'
export type StaffNavigationItem = {
  id: StaffSection
  label: string
  group: 'General' | 'Operación' | 'Inventario' | 'Administración'
  description: string
  roles: readonly StaffRole[]
}

const staff: readonly StaffRole[] = ['ADMINISTRADOR', 'VENDEDOR']
const admin: readonly StaffRole[] = ['ADMINISTRADOR']

export const staffNavigation: readonly StaffNavigationItem[] = [
  { id: 'inicio', label: 'Inicio', group: 'General', description: 'Tu espacio de trabajo diario.', roles: staff },
  { id: 'ventas', label: 'Ventas', group: 'Operación', description: 'Nueva venta, historial y operaciones autorizadas.', roles: staff },
  { id: 'caja', label: 'Caja', group: 'Operación', description: 'Apertura, movimientos y cierre de caja.', roles: staff },
  { id: 'pedidos', label: 'Pedidos', group: 'Operación', description: 'Preparación, entrega a domicilio y retiro en el negocio.', roles: staff },
  { id: 'productos', label: 'productos', group: 'Inventario', description: '', roles: staff },
  { id: 'compras', label: 'Compras', group: 'Inventario', description: 'Registro de compras y seguimiento de pagos.', roles: admin },
  { id: 'gastos', label: 'Gastos', group: 'Administración', description: 'Registro y consulta de gastos del negocio.', roles: admin },
  { id: 'reportes', label: 'Reportes', group: 'Administración', description: 'Consulta de ventas, caja e inventario.', roles: admin },
  { id: 'configuracion', label: 'Configuración', group: 'Administración', description: 'Datos del negocio, usuarios, roles y permisos.', roles: admin },
]

export function getStaffNavigation(role: StaffRole) {
  return staffNavigation.filter(item => item.roles.includes(role))
}