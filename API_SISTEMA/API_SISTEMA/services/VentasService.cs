using API_SISTEMA.data;
using API_SISTEMA.DTOs.Ventas;
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
                    Cliente cliente;

                    if (ventaDto.id_cliente > 0)
                    {
                        cliente = await _context.cliente
                            .FirstOrDefaultAsync(c => c.id_Cliente == ventaDto.id_cliente);

                        if (cliente == null)
                            throw new Exception("El cliente seleccionado no existe.");
                    }
                    else
                    {
                        if (ventaDto.clienteNuevo == null)
                            throw new Exception("Debe ingresar los datos del cliente nuevo.");

                        cliente = new Cliente
                        {
                            nombre = ventaDto.clienteNuevo.nombre,
                            apellido = ventaDto.clienteNuevo.apellido,
                            nit = ventaDto.clienteNuevo.nit,
                            dpi = ventaDto.clienteNuevo.dpi,
                            telefono = ventaDto.clienteNuevo.telefono,
                            correo_electronico = ventaDto.clienteNuevo.correo_electronico,
                            direccion = ventaDto.clienteNuevo.direccion,
                            limite_Credito = ventaDto.clienteNuevo.limite_Credito,
                            estado = true
                        };

                        _context.cliente.Add(cliente);
                        await _context.SaveChangesAsync();
                    }

                    if (ventaDto.detalles == null || ventaDto.detalles.Count == 0)
                        throw new Exception("La venta debe tener al menos un producto.");

                    if (ventaDto.pago == null)
                        throw new Exception("Debe ingresar el pago inicial.");

                    decimal subtotalVenta = 0;
                    decimal descuentoTotal = 0;

                    var venta = new Ventas
                    {
                        id_cliente = cliente.id_Cliente,
                        id_usuario = ventaDto.id_usuario,
                        observacion = ventaDto.observacion_pago,
                        origen = ventaDto.origen,
                        fecha_venta = DateTime.Now,

                        subtotal = 0,
                        descuento = 0,
                        total = 0,

                        monto_pagado = ventaDto.pago.monto_pagado,
                        saldo_pendiente = 0,

                        id_estado_venta = 1
                    };

                    _context.ventas.Add(venta);
                await _context.SaveChangesAsync();


                decimal gananciaTotal = 0;
                //recorre los elementso que viene desde el frotend

                foreach (var detalle in ventaDto.detalles)
                {
                    var producto = await _context.productos
                        .FirstOrDefaultAsync(p => p.id_producto == detalle.id_producto);

                    if (producto == null)
                        throw new Exception("El producto no existe.");
                    //consulta el producto y presentacion al mismo tiempo
                    var presentacion = await _context.producto_presentaciones
                        .FirstOrDefaultAsync(p =>
                            p.id_producto_presentacion == detalle.id_producto_presentacion &&
                            p.id_producto == detalle.id_producto);

                    if (presentacion == null)
                        throw new Exception("La presentación seleccionada no existe para este producto.");

                    int unidadesADescontar = detalle.cantidad * presentacion.unidades_equivalentes;

                    if (producto.stock < unidadesADescontar)
                        throw new Exception($"Stock insuficiente para el producto {producto.nombre}.");

                    decimal subtotalDetalle = detalle.cantidad * presentacion.precio;
                    decimal descuentoDetalle = detalle.descuento;
                    decimal totalDetalle = subtotalDetalle - descuentoDetalle;

                    decimal costoDetalle = detalle.cantidad * presentacion.unidades_equivalentes * producto.costo_unitario;

                    decimal gananciaDetalle = totalDetalle - costoDetalle;

                    //descontar la cantidad de producto vendido
                    producto.stock -= unidadesADescontar;

                    var detalleVenta = new Detalle_venta
                    {

                        id_venta = venta.id_ventas,
                        id_producto = detalle.id_producto,
                        id_producto_presentacion = detalle.id_producto_presentacion,
                        cantidad = detalle.cantidad,
                       // precio_unitario = presentacion.precio,
                        descuento = descuentoDetalle,
                        subtotal = totalDetalle,
                        ganancia = gananciaDetalle
                    };

                    _context.detalle_Ventas.Add(detalleVenta);

                    subtotalVenta += subtotalDetalle;
                    descuentoTotal += descuentoDetalle;
                    gananciaTotal += gananciaDetalle;
                }

                venta.subtotal = subtotalVenta;
                venta.descuento = descuentoTotal;
                venta.total = subtotalVenta - descuentoTotal;
                venta.ganancia_total = gananciaTotal;

                if (venta.monto_pagado < 0)
                    throw new Exception("El monto pagado no puede ser negativo.");

                venta.saldo_pendiente = venta.total - venta.monto_pagado;

                if (venta.saldo_pendiente <= 0)
                {
                    venta.saldo_pendiente = 0;
                    venta.id_estado_venta = 1;
                }
                else
                {
                    venta.id_estado_venta = 2;
                }

                if (ventaDto.pago.monto_pagado > 0)
                {
                    var pago = new Pagos
                    {
                        id_venta = venta.id_ventas,
                        id_usuario = (int)ventaDto.id_usuario,
                        monto = ventaDto.pago.monto_pagado,
                        metodo_pago = ventaDto.pago.metodo_pago,
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

