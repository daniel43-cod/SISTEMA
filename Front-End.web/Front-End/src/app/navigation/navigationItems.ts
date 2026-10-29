import type { StaffRole } from '../../features/auth/types/auth'

export type StaffSection = 'ventas' | 'listado-ventas' | 'caja' | 'pedidos' | 'productos' | 'inventario' | 'compras' | 'gastos' | 'usuarios' | 'reportes' | 'configuracion'
export type StaffNavigationItem = {
  id: StaffSection
  label: string
  group: 'Operación' | 'Inventario' | 'Administración'
  description: string
  roles: readonly StaffRole[]
}

const staff: readonly StaffRole[] = ['ADMINISTRADOR', 'VENDEDOR']
const admin: readonly StaffRole[] = ['ADMINISTRADOR']

export const staffNavigation: readonly StaffNavigationItem[] = [
  { id: 'ventas', label: 'Nueva venta', group: 'Operación', description: 'Registrar una venta.', roles: staff },
  { id: 'listado-ventas', label: 'Ventas', group: 'Operación', description: '', roles: staff },
  { id: 'caja', label: 'Caja', group: 'Operación', description: 'Consulta y apertura de la caja compartida.', roles: admin },
  { id: 'pedidos', label: 'Pedidos', group: 'Operación', description: 'Pedidos pendientes del catálogo y sus datos de entrega.', roles: admin },
  { id: 'productos', label: 'Productos', group: 'Inventario', description: '', roles: staff },
  { id: 'inventario', label: 'Inventario', group: 'Inventario', description: 'Existencias y precio promedio de compra por producto.', roles: admin },
  { id: 'compras', label: 'Compras', group: 'Inventario', description: '', roles: admin },
  { id: 'gastos', label: 'Gastos', group: 'Administración', description: 'Registro y consulta de gastos del negocio.', roles: admin },
  { id: 'usuarios', label: 'Usuarios', group: 'Administración', description: 'Consulta y creación de usuarios internos.', roles: admin },
  { id: 'reportes', label: 'Reportes', group: 'Administración', description: 'Consulta de ventas, caja e inventario.', roles: admin },
  { id: 'configuracion', label: 'Configuración', group: 'Administración', description: 'Datos del negocio, usuarios, roles y permisos.', roles: admin },
]

export function getStaffNavigation(role: StaffRole) {
  return staffNavigation.filter(item => item.roles.includes(role))
}
