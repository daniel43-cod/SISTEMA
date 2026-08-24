using API_SISTEMA.data;
using API_SISTEMA.DTOs.Ventas;
using API_SISTEMA.models;
using API_SISTEMA.services.MovimientoCaja;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;
using API_SISTEMA.DTOs;
using API_SISTEMA.services.Permisos;

namespace API_SISTEMA.services.Ventas
{
    public class CrearVentaService
    {
        private readonly SistemaDbContext _context;
        private readonly MovimientoCajaService _movimientoCajaService;  
        private readonly PermisoUsuarioService _permisoService;

        public CrearVentaService(SistemaDbContext context, MovimientoCajaService movimientoCajaService, PermisoUsuarioService permisoService)
        {
            _context = context;
            _movimientoCajaService = movimientoCajaService;
            _permisoService = permisoService    ;
        }

        public async Task<API_SISTEMA.models.Ventas> CrearVenta(CrearVentaDTO ventaDto, int idUsuario)
        {

            bool puedeVender = await _permisoService.TienePermiso(idUsuario, permisos.Vender);
            if( !puedeVender)
                throw new Exception("No tienes permiso para realizar ventas.");


            if (ventaDto == null) throw new Exception("La información de la venta es obligatoria.");

            if (ventaDto.detalles == null || ventaDto.detalles.Count == 0) throw new Exception("La venta debe tener al menos un producto.");

            // 1. Buscar sesión de caja abierta del usuario autenticado
            var sesionCaja = await _context.sesioncaja
                .FirstOrDefaultAsync(s =>
                    s.id_usuario_apertura == idUsuario &&
                    s.fecha_cierre == null
                );

            if (sesionCaja == null)
            {
                throw new Exception(
                    "Debes abrir una caja para poder realizar la venta."
                );
            }

            using var transaction =await _context.Database.BeginTransactionAsync();

            try
            {
                // 2. Obtener o crear cliente
                Cliente cliente;

                if (ventaDto.id_cliente > 0)
                {
                    cliente = await _context.cliente
                        .FirstOrDefaultAsync(c =>
                            c.id_cliente == ventaDto.id_cliente
                        );

                    if (cliente == null)
                        throw new Exception("El cliente seleccionado no existe.");
                }
                else
                {
                    if (ventaDto.clienteNuevo == null)
                        throw new Exception("Debe ingresar los datos del cliente.");

                    cliente = new Cliente
                    {
                        nombre = ventaDto.clienteNuevo.nombre,
                        apellido = ventaDto.clienteNuevo.apellido,
                        nit = ventaDto.clienteNuevo.nit,
                        dpi = ventaDto.clienteNuevo.dpi,
                        telefono = ventaDto.clienteNuevo.telefono,
                        correo_electronico =
                            ventaDto.clienteNuevo.correo_electronico,
                        direccion = ventaDto.clienteNuevo.direccion,
                        estado = true
                    };

                    _context.cliente.Add(cliente);

                    await _context.SaveChangesAsync();
                }

                // 3. Crear cabecera inicialmente
                var venta = new API_SISTEMA.models.Ventas
                {
                    id_cliente = cliente.id_cliente,
                    id_usuario = idUsuario,
                    id_sesion_caja =sesionCaja.id_sesion_caja,
                    fecha_venta = DateTime.Now,
                    origen = ventaDto.origen,
                    observacion = ventaDto.observacion,
                    subtotal = 0,
                    descuento = 0,
                    impuesto = 0,
                    total = 0,
                    ganancia_total = 0,
                 
                    monto_pagado = 0,
                    saldo_pendiente = 0,
                    id_estado_venta = 2
                };

                _context.ventas.Add(venta);

                await _context.SaveChangesAsync();

                decimal subtotalVenta = 0m;
                decimal descuentoTotal = 0m;
                decimal gananciaTotal = 0m;

                // 4. Procesar detalles
                foreach (var detalleDto in ventaDto.detalles)
                {
                    if (detalleDto.cantidad <= 0)throw new Exception("La cantidad debe ser mayor que cero.");

                    if (detalleDto.descuento < 0)throw new Exception("El descuento no puede ser negativo.");

                    var producto = await _context.productos.FirstOrDefaultAsync(p =>p.id_producto == detalleDto.id_producto);

                    if (producto == null)throw new Exception("El producto no existe.");

                    var presentacion =await _context.producto_presentaciones.FirstOrDefaultAsync(p =>
                                p.id_producto_presentacion ==detalleDto.id_producto_presentacion &&
                                p.id_producto ==detalleDto.id_producto);

                    if (presentacion == null)
                    {
                        throw new Exception(
                            $"La presentación no existe para " +
                            $"{producto.nombre}."
                        );
                    }

                    int unidadesADescontar =detalleDto.cantidad * presentacion.unidades_equivalentes;

                    if (producto.stock < unidadesADescontar)
                    {
                        throw new Exception(
                            $"Stock insuficiente para {producto.nombre}."
                        );
                    }

                    decimal subtotalDetalle =
                        detalleDto.cantidad * presentacion.precio;

                    if (detalleDto.descuento > subtotalDetalle)
                    {
                        throw new Exception(
                            $"El descuento de {producto.nombre} " +
                            $"no puede superar el subtotal."
                        );
                    }

                    decimal totalDetalle =subtotalDetalle - detalleDto.descuento;
                    decimal costoUnitario =producto.costo_unitario ?? 0m;
                    decimal costoDetalle =detalleDto.cantidad *presentacion.unidades_equivalentes *costoUnitario;
                    decimal gananciaDetalle = totalDetalle - costoDetalle;

                    producto.stock -= unidadesADescontar;

                    var detalleVenta = new Detalle_venta
                    {
                        id_venta = venta.id_ventas,
                        id_producto =detalleDto.id_producto,
                        id_producto_presentacion = detalleDto.id_producto_presentacion,
                        cantidad =detalleDto.cantidad,
                        descuento =detalleDto.descuento,
                        subtotal =totalDetalle,
                        ganancia =gananciaDetalle
                    };

                    _context.detalle_Ventas.Add(detalleVenta);

                    subtotalVenta += subtotalDetalle;
                    descuentoTotal += detalleDto.descuento;
                    gananciaTotal += gananciaDetalle;
                }

                // 5. Totales de la venta
                venta.subtotal = subtotalVenta;
                venta.descuento = descuentoTotal;

                venta.total =
                    subtotalVenta - descuentoTotal;

                venta.ganancia_total =
                    gananciaTotal;

                // 6. Registrar pago inicial, si existe
                Pagos? pagoCreado = null;

                if (ventaDto.pago != null &&
                    ventaDto.pago.monto > 0)
                {
                    if (ventaDto.pago.monto > venta.total)
                    {
                        throw new Exception(
                            "El monto pagado no puede superar " +
                            "el total de la venta."
                        );
                    }

                    pagoCreado = new Pagos
                    {
                        id_venta = venta.id_ventas,
                        id_usuario = idUsuario,
                        id_sesion_caja =sesionCaja.id_sesion_caja,
                        monto =ventaDto.pago.monto,
                        metodo_pago =ventaDto.pago.metodo_pago,
                        observacion = ventaDto.pago.observacion,
                        fecha_pago = DateTime.Now
                    };

                    _context.pagos.Add(pagoCreado);

                    await _context.SaveChangesAsync();

                    // 7. Registrar movimiento de caja
                    // Solo si realmente afecta efectivo
                   
                        await _movimientoCajaService .RegistrarMovimiento(
                                idSesionCaja:sesionCaja.id_sesion_caja,
                                idUsuario:idUsuario,
                                idTipoMovimiento:TiposMovimientoCaja.Venta,
                                monto:ventaDto.pago.monto,
                                descripcion:$"Pago de venta #{venta.id_ventas}",
                                idVenta:venta.id_ventas,
                                idPagoVenta:pagoCreado.id_pago
                            );
                    
                }

                // 8. Calcular monto pagado DESDE pagos
                decimal totalPagado =await _context.pagos .Where(p => p.id_venta == venta.id_ventas)   .SumAsync(p => (decimal?)p.monto)?? 0m;

                venta.monto_pagado =totalPagado;

                venta.saldo_pendiente = venta.total - totalPagado;

                if (venta.saldo_pendiente <= 0)
                {
                    venta.saldo_pendiente = 0;
                    venta.id_estado_venta = 1;
                }
                else
                {
                    venta.id_estado_venta = 2;
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return venta;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
