using API_SISTEMA.data;
using API_SISTEMA.DTOs;
using API_SISTEMA.models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Contracts;
using System.Reflection.Metadata.Ecma335;
using static System.Net.Mime.MediaTypeNames;

namespace API_SISTEMA.services
{
    public class VentaService
    {
        private readonly SistemaDbContext _context;

        public VentaService(SistemaDbContext context)
        {
            _context = context;
            
        }

        
        public async Task<List<Ventas>> ListarVentes()
        {
            return await _context.ventas.ToListAsync();
        }
        //crear venta nueva

        public async Task<Ventas> CrearVenta(VentasDTOs ventaDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var cliente = await _context.cliente
                    .FirstOrDefaultAsync(c => c.id_Cliente == ventaDto.id_cliente);

                if (cliente == null)
                    throw new Exception("El cliente seleccionado no existe.");

                if (ventaDto.detalles == null || ventaDto.detalles.Count == 0)
                    throw new Exception("La venta debe tener al menos un producto.");

                if (ventaDto.pago == null)
                    throw new Exception("Debe ingresar el pago inicial.");

                decimal subtotalVenta = 0;
                decimal descuentoTotal = 0;

                var venta = new Ventas
                {
                    id_cliente = ventaDto.id_cliente,
                    id_usuario = ventaDto.id_usuario,
                    id_tipo_cliente = ventaDto.id_tipo_cliente,
                    observacion = ventaDto.observacion_pago,
                    origen = ventaDto.origen,
                    fecha_venta = DateTime.Now,

                    subtotal = 0,
                    descuento = 0,
                    total = 0,

                    monto_pagado = ventaDto.pago.monto_pagado,
                    saldo_pendiente = 0,

                    id_estado_venta = 1 // temporal, evita que se guarde 0
                };

                _context.ventas.Add(venta);
                await _context.SaveChangesAsync();

                foreach (var item in ventaDto.detalles)
                {
                    var producto = await _context.productos
                        .FirstOrDefaultAsync(p => p.id_producto == item.id_producto);

                    if (producto == null)
                        throw new Exception($"El producto con ID {item.id_producto} no existe.");

                    if (producto.stock < item.cantidad)
                        throw new Exception($"Stock insuficiente para el producto {producto.nombre}.");

                    var precioProducto = await _context.producto_precios
                        .FirstOrDefaultAsync(p =>
                            p.id_producto == item.id_producto &&
                            p.id_tipo_cliente == ventaDto.id_tipo_cliente &&
                            p.estado == true);

                    if (precioProducto == null)
                        throw new Exception($"No existe precio para el producto {producto.nombre} según el tipo de cliente.");

                    decimal subtotalBruto = item.cantidad * precioProducto.precio;
                    decimal descuentoDetalle = (decimal)item.descuento;
                    decimal subtotalNeto = subtotalBruto - descuentoDetalle;

                    if (descuentoDetalle < 0)
                        throw new Exception("El descuento no puede ser negativo.");

                    if (descuentoDetalle > subtotalBruto)
                        throw new Exception($"El descuento del producto {producto.nombre} no puede ser mayor al subtotal.");

                    var detalle = new Detalle_venta
                    {
                        id_venta = venta.id_ventas,
                        id_producto = item.id_producto,
                        cantidad = item.cantidad,
                        precio = precioProducto.precio,
                        descuento = descuentoDetalle,
                        subtotal = subtotalNeto
                    };

                    _context.detalle_Ventas.Add(detalle);

                    producto.stock -= item.cantidad;

                    subtotalVenta += subtotalBruto;
                    descuentoTotal += descuentoDetalle;
                }

                venta.subtotal = subtotalVenta;
                venta.descuento = descuentoTotal;
                venta.total = subtotalVenta - descuentoTotal;

                if (venta.monto_pagado < 0)
                    throw new Exception("El monto pagado no puede ser negativo.");

                venta.saldo_pendiente = venta.total - venta.monto_pagado;

                if (venta.saldo_pendiente <= 0)
                {
                    venta.saldo_pendiente = 0;
                    venta.id_estado_venta = 1; // Cancelada
                }
                else
                {
                    venta.id_estado_venta = 2; // Pendiente / Crédito
                }

                if (ventaDto.pago.monto_pagado > 0)
                {
                    var pago = new Pagos
                    {
                        id_venta = venta.id_ventas,
                        id_usuario = (int)ventaDto.id_usuario,
                        monto = ventaDto.pago.monto_pagado,
                        metodo_pago  = ventaDto.pago.metodo_pago,
                        observacion = ventaDto.observacion_pago,
                        fecha_pago = DateTime.Now
                    };

                    _context.pagos.Add(pago);
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

