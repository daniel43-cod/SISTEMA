export interface Supplier { idProveedor: number; nombre: string }
export interface Purchase {
  id_compra: number; nombre_usuario: string; nombre_proveedor: string
  descripcion_estado_compra: string; fecha_ingreso: string; total_compra: number; saldo_pendiente: number
}
export interface PurchaseDetail {
  id_detalle_compra: number; nombre_producto: string; cantidad: number; subtotal: number
}
export interface PurchaseLine { idProducto: number; nombre: string; cantidad: string; total: string }
export interface CreatePurchase {
  id_proveedor: number; observacion: string | null; monto_pagado: number
  detalle_compra: { id_producto: number; cantidad: number; precio: number }[]
}
export interface CreatedPurchase {
  id_compra: number; total_compra: number; saldo_pendiente: number
  productos: { idProducto: number; stock: number; precioPromedioCompra: number }[]
}
