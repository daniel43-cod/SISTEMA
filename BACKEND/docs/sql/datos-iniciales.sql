-- Catálogos necesarios para una instalación nueva. No contiene operaciones ni usuarios de prueba.
BEGIN;
SELECT pg_advisory_xact_lock(682719043);
INSERT INTO public.rol (id_rol,nombre,descripcion,estado) VALUES
 (1,'ADMINISTRADOR','Administración del sistema',true),(2,'VENDEDOR','Operación de ventas',true)
 ON CONFLICT (id_rol) DO NOTHING;
INSERT INTO public.estado_venta (id_estado_venta,descripcion,estado) VALUES
 (1,'Pagada',true),(2,'Pendiente',true) ON CONFLICT (id_estado_venta) DO NOTHING;
INSERT INTO public.estado_compra (id_estado_compra,nombre_estado_compra,descripcion) VALUES
 (1,'Pagada','Compra pagada'),(2,'Pendiente','Compra con saldo pendiente') ON CONFLICT (id_estado_compra) DO NOTHING;
INSERT INTO public.tabla_permisos (id_permiso,nombre,descripcion,estado) VALUES
 (1,'Vender','Registrar ventas',true),(2,'Descuento','Aplicar descuentos',true),
 (3,'ActualizarProducto','Actualizar productos',true),(4,'VerReporte','Consultar reportes',true),
 (5,'RegistrarAbonoPago','Registrar abonos de ventas',true),(6,'RegistrarAbonoCompra','Registrar abonos de compras',true),
 (7,'RegistrarCompra','Registrar compras',true),(8,'RegistrarGasto','Registrar gastos',true),
 (9,'VerInteres','Consultar interés',true),(10,'ModificarPrecioProducto','Modificar precios de productos',true)
 ON CONFLICT (id_permiso) DO NOTHING;
INSERT INTO public.tipo_movimiento_caja (id_tipo_movimiento,nombre_movimiento,naturaleza) VALUES
 (10,'Venta','ENTRADA'),(12,'Abono de venta','ENTRADA'),(14,'Pago de compra','SALIDA'),(16,'Gasto','SALIDA'),(18,'Abono de compra','SALIDA')
 ON CONFLICT (id_tipo_movimiento) DO NOTHING;
-- Solo el rol administrador recibe permisos iniciales. Los del vendedor se asignan explícitamente en el panel.
INSERT INTO public.rol_permiso (id_rol,id_permiso)
 SELECT 1,p.id_permiso FROM public.tabla_permisos p
 WHERE NOT EXISTS (SELECT 1 FROM public.rol_permiso r WHERE r.id_rol=1 AND r.id_permiso=p.id_permiso);
-- Los INSERT con IDs fijos no adelantan por sí solos las secuencias de identidad.
SELECT setval(pg_get_serial_sequence('public.rol','id_rol'),(SELECT max(id_rol) FROM public.rol));
SELECT setval(pg_get_serial_sequence('public.estado_venta','id_estado_venta'),(SELECT max(id_estado_venta) FROM public.estado_venta));
SELECT setval(pg_get_serial_sequence('public.estado_compra','id_estado_compra'),(SELECT max(id_estado_compra) FROM public.estado_compra));
SELECT setval(pg_get_serial_sequence('public.tabla_permisos','id_permiso'),(SELECT max(id_permiso) FROM public.tabla_permisos));
SELECT setval(pg_get_serial_sequence('public.tipo_movimiento_caja','id_tipo_movimiento'),(SELECT max(id_tipo_movimiento) FROM public.tipo_movimiento_caja));
COMMIT;