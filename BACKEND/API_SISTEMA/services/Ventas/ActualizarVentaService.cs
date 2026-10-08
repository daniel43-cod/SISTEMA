using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Ventas;
using API_SISTEMA.Models;
using Microsoft.EntityFrameworkCore;
namespace API_SISTEMA.Services.Ventas
{
    public class ActualizarVentaService
    {
        private readonly SistemaDbContext _context;

        public ActualizarVentaService(SistemaDbContext context)
        {
            _context = context;
        }

        public async Task ModificarVenta(int idVenta,ModificarVentaDto dto,int idUsuario)
        {
            if (dto == null)
                throw new Exception(
                    "La información de la venta es obligatoria."
                );

            if (dto.detalles == null || dto.detalles.Count == 0)
                throw new Exception(
                    "La venta debe contener al menos un producto."
                );

            using var transaccion =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var venta = await _context.ventas
                    .Include(v => v.DetalleVentas)
                    .FirstOrDefaultAsync(v =>
                        v.id_ventas == idVenta
                    );

                if (venta == null)
                    throw new Exception(
                        "La venta no existe."
                    );

                // =====================================================
                // 2. VALIDAR QUE NO ESTÉ CERTIFICADA
                // =====================================================

                // ADAPTAR al nombre real de tu entidad/propiedad FEL.
                //
                // Ejemplo:
                //
                // bool certificada = await _context.factura_fel
                //     .AnyAsync(f =>
                //         f.id_venta == idVenta &&
                //         f.estado == "CERTIFICADA");
                //
                // if (certificada)
                // {
                //     throw new Exception(
                //         "La venta ya fue certificada y no puede modificarse."
                //     );
                // }

                // =====================================================
                // 3. VALIDAR CLIENTE
                // =====================================================

                bool clienteExiste = await _context.cliente
                    .AnyAsync(c =>
                        c.id_cliente == dto.id_cliente
                    );

                if (!clienteExiste)
                    throw new Exception(
                        "El cliente indicado no existe."
                    );

                // =====================================================
                // 4. OBTENER PRESENTACIONES NUEVAS
                // =====================================================

                var idsPresentaciones = dto.detalles
                    .Select(d => d.id_producto_presentacion)
                    .Distinct()
                    .ToList();

                var presentaciones = await _context.producto_presentaciones
                    .Include(p => p.Producto)
                    .Where(p =>
                        idsPresentaciones.Contains(
                            p.id_producto_presentacion
                        ) && p.estado && p.Presentacion.Estado == true
                    )
                    .ToListAsync();

                var presentacionesPorId = presentaciones
                    .ToDictionary(
                        p => p.id_producto_presentacion
                    );

                // =====================================================
                // 5. DEVOLVER STOCK DE LA VENTA ACTUAL
                // =====================================================

                foreach (var detalleActual in venta.DetalleVentas)
                {
                    var presentacionActual =
                        await _context.producto_presentaciones
                            .FirstOrDefaultAsync(p =>
                                p.id_producto_presentacion ==
                                detalleActual.id_producto_presentacion
                            );

                    if (presentacionActual == null)
                        throw new Exception(
                            "No se encontró una presentación actual de la venta."
                        );

                    var productoActual =
                        await _context.productos
                            .FirstAsync(p =>
                                p.id_producto ==
                                detalleActual.id_producto
                            );

                    int unidadesDevueltas =
                        detalleActual.cantidad *
                        presentacionActual.unidades_equivalentes;

                    productoActual.stock =
                        (productoActual.stock ?? 0) +
                        unidadesDevueltas;
                }

                // =====================================================
                // 6. VALIDAR NUEVOS DETALLES Y CALCULAR TOTALES
                // =====================================================

                decimal subtotal = 0;
                decimal descuentoTotal = 0;
                decimal impuestoTotal = 0;
                decimal gananciaTotal = 0;

                foreach (var detalleDto in dto.detalles)
                {
                    if (detalleDto.cantidad <= 0)
                        throw new Exception(
                            "La cantidad debe ser mayor a cero."
                        );

                    if (detalleDto.descuento < 0)
                        throw new Exception(
                            "El descuento no puede ser negativo."
                        );

                    if (!presentacionesPorId.TryGetValue(
                            detalleDto.id_producto_presentacion,
                            out var presentacion))
                    {
                        throw new Exception(
                            $"La presentación " +
                            $"{detalleDto.id_producto_presentacion} no existe."
                        );
                    }

                    if (presentacion.id_producto !=
                        detalleDto.id_producto)
                    {
                        throw new Exception(
                            "La presentación no pertenece al producto indicado."
                        );
                    }

                    var producto = presentacion.Producto;

                    int unidadesADescontar =
                        detalleDto.cantidad *
                        presentacion.unidades_equivalentes;

                    int stockDisponible =
                        producto.stock ?? 0;

                    if (unidadesADescontar > stockDisponible)
                    {
                        throw new Exception(
                            $"Stock insuficiente para " +
                            $"{producto.nombre}."
                        );
                    }

                    decimal subtotalDetalle =
                        detalleDto.cantidad *
                        presentacion.precio;

                    if (detalleDto.descuento > subtotalDetalle)
                    {
                        throw new Exception(
                            $"El descuento de {producto.nombre} " +
                            "no puede superar el subtotal."
                        );
                    }

                    subtotal += subtotalDetalle;
                    descuentoTotal += detalleDto.descuento;

                    decimal costoDetalle =detalleDto.cantidad *presentacion.unidades_equivalentes * (producto.costo_unitario ?? 0)  ;

                    decimal ventaDetalle =
                        subtotalDetalle -
                        detalleDto.descuento;

                    gananciaTotal +=
                        ventaDetalle - costoDetalle;

                    producto.stock =
                        stockDisponible - unidadesADescontar;
                }

                decimal total =
                    subtotal - descuentoTotal + impuestoTotal;

                // =====================================================
                // 7. CALCULAR CUÁNTO YA SE HA PAGADO
                // =====================================================

                decimal montoPagado = await _context.pagos
                    .Where(p =>
                        p.id_venta == idVenta
                    )
                    .SumAsync(p =>
                        (decimal?)p.monto
                    ) ?? 0;

                // =====================================================
                // 8. EVITAR QUE EL NUEVO TOTAL SEA MENOR
                //    QUE LO YA PAGADO SIN RESOLVER DEVOLUCIÓN
                // =====================================================

                if (montoPagado > total)
                {
                    decimal excedente =
                        montoPagado - total;

                    throw new Exception(
                        $"La venta ya tiene Q{montoPagado:N2} pagados, " +
                        $"pero el nuevo total sería Q{total:N2}. " +
                        $"Existe un excedente de Q{excedente:N2} " +
                        "que debe resolverse antes de modificar la venta."
                    );
                }

                decimal saldoPendiente =
                    total - montoPagado;

                // =====================================================
                // 9. ACTUALIZAR CABECERA DE VENTA
                // =====================================================

                venta.id_cliente =
                    dto.id_cliente;

                venta.subtotal =
                    subtotal;

                venta.descuento =
                    descuentoTotal;

                venta.impuesto =
                    impuestoTotal;

                venta.total =
                    total;

                venta.monto_pagado =
                    montoPagado;

                venta.saldo_pendiente =
                    saldoPendiente;

                venta.ganancia_total =
                    gananciaTotal;

                venta.observacion =
                    dto.observacion;

                // Ajusta estos IDs a los estados reales de tu BD
                venta.id_estado_venta =
                    saldoPendiente == 0
                        ? 1
                        : 2;

                // =====================================================
                // 10. REEMPLAZAR DETALLES
                // =====================================================

                _context.detalle_Ventas.RemoveRange(
                    venta.DetalleVentas
                );

                foreach (var detalleDto in dto.detalles)
                {
                    var presentacion =
                        presentacionesPorId[
                            detalleDto.id_producto_presentacion
                        ];

                    decimal subtotalDetalle =
                        detalleDto.cantidad *
                        presentacion.precio;    

                    var nuevoDetalle = new DetalleVenta
                    {
                        id_venta =
                            venta.id_ventas,

                        id_producto =
                            detalleDto.id_producto,

                        id_producto_presentacion =
                            detalleDto.id_producto_presentacion,

                        cantidad =
                            detalleDto.cantidad,

                        precio =
                            presentacion.precio,

                        descuento =
                            detalleDto.descuento,

                        subtotal =
                            subtotalDetalle -
                            detalleDto.descuento
                    };

                    _context.detalle_Ventas.Add(
                        nuevoDetalle
                    );
                }

                // =====================================================
                // 11. GUARDAR TODO
                // =====================================================

                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }
    }
}
