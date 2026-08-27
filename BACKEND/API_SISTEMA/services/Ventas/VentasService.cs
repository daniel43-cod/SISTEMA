using API_SISTEMA.data;
using API_SISTEMA.DTOs.Catalogo;
using API_SISTEMA.DTOs.Ventas;
using API_SISTEMA.models;
using API_SISTEMA.Utilidades;
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

        public async Task<List<ListarVentasDTOs>> ListarVentas(int idUsuario)
        {
            var usuario = await _context.usuarios
                .FirstOrDefaultAsync(u => u.id_usuario == idUsuario);

            if (usuario == null)
            {
                throw new Exception("Usuario no encontrado.");
            }

            var sesionCaja = await _context.sesioncaja
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.id_usuario_apertura == idUsuario &&
                    s.fecha_cierre == null
                );

            if (sesionCaja == null)
            {
                return new List<ListarVentasDTOs>();
            }

            return await _context.ventas
                .AsNoTracking()
                .Where(v =>
                    v.id_sesion_caja == sesionCaja.id_sesion_caja
                )
                .OrderByDescending(v => v.fecha_venta)
                .Select(v => new ListarVentasDTOs
                {
                    id_ventas = v.id_ventas,
                    subtotal = v.subtotal,
                    descuento = v.descuento,
                    impuesto = v.impuesto,
                    total = v.total,
                    fecha_venta = v.fecha_venta,
                    id_cliente = v.id_cliente,
                    cliente = v.cliente.nombre,
                    id_usuario = v.id_usuario,
                    usuario = v.usuario.nombre,
                    estado = v.EstadoVenta.descripcion,
                    origen = v.origen,
                    ganancia_total = v.ganancia_total,
                    monto_pagado = v.monto_pagado,
                    saldo_pendiente = v.saldo_pendiente,
                    observacion = v.observacion
                })
                .ToListAsync();
        }

        public async Task<API_SISTEMA.models.Ventas> CrearVenta(VentasDTOs ventaDto, int idUsuario)
        {
           
            var sesionCaja = await _context.sesioncaja.FirstOrDefaultAsync(s =>s.id_usuario_apertura == idUsuario && s.fecha_cierre == null);

            if (sesionCaja == null)
            {
                throw new Exception(
                    "Debes abrir una caja para poder realizar la venta.");
            }

            if (ventaDto.detalles == null || ventaDto.detalles.Count == 0)
                throw new Exception("La venta debe tener al menos un producto.");

            if (ventaDto.pago == null)
                throw new Exception("Debe ingresar el pago inicial.");

            if (ventaDto.pago.monto_pagado < 0)
                throw new Exception("El monto pagado no puede ser negativo.");

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                Cliente cliente;

                if (ventaDto.id_cliente > 0)
                {
                    cliente = await _context.cliente
                        .FirstOrDefaultAsync(c => c.id_cliente == ventaDto.id_cliente);

                    if (cliente == null)
                        throw new Exception(
                            "El cliente seleccionado no existe.");
                }
                else
                {
                    if (ventaDto.clienteNuevo == null)
                    {
                        throw new Exception("Debe ingresar los datos del cliente nuevo.");
                    }

                    cliente = new Cliente
                    {
                        nombre = ventaDto.clienteNuevo.nombre,
                        apellido = ventaDto.clienteNuevo.apellido,
                        nit = ventaDto.clienteNuevo.nit,
                        dpi = ventaDto.clienteNuevo.dpi,
                        telefono = ventaDto.clienteNuevo.telefono,
                        correo_electronico = ventaDto.clienteNuevo.correo_electronico,
                        direccion = ventaDto.clienteNuevo.direccion,
                        estado = true
                    };

                    _context.cliente.Add(cliente);
                    await _context.SaveChangesAsync();

                }

                var venta = new API_SISTEMA.models.Ventas
                {
                    id_cliente = cliente.id_cliente,

                    // Datos obtenidos internamente
                    id_usuario = idUsuario,
                    id_sesion_caja = sesionCaja.id_sesion_caja,

                    observacion = ventaDto.observacion_pago,
                    origen = ventaDto.origen,
                    fecha_venta = DateTime.Now,

                    subtotal = 0,
                    descuento = 0,
                    total = 0,
                    ganancia_total = 0,

                    //monto_pagado = ventaDto.pago.monto_pagado,
                    saldo_pendiente = 0,

                    id_estado_venta = 1
                };

                _context.ventas.Add(venta);
                await _context.SaveChangesAsync();

                decimal subtotalVenta = 0;
                decimal descuentoTotal = 0;
                decimal gananciaTotal = 0;

                foreach (var detalle in ventaDto.detalles)
                {
                    if (detalle.cantidad <= 0)
                    {
                        throw new Exception(
                            "La cantidad de los productos debe ser mayor que cero.");
                    }

                    if (detalle.descuento < 0)
                    {
                        throw new Exception(
                            "El descuento no puede ser negativo.");
                    }

                    var producto = await _context.productos.FirstOrDefaultAsync(p => p.id_producto == detalle.id_producto);

                    if (producto == null)
                    throw new Exception("El producto no existe.");

                    var presentacion =await _context.producto_presentaciones.FirstOrDefaultAsync(p =>
                                p.id_producto_presentacion ==detalle.id_producto_presentacion &&
                                p.id_producto == detalle.id_producto);

                    if (presentacion == null)
                    {
                        throw new Exception(
                            $"La presentación seleccionada no existe para el producto {producto.nombre}.");
                    }

                    int unidadesADescontar =detalle.cantidad * presentacion.unidades_equivalentes;

                    if (producto.stock < unidadesADescontar)
                    {
                        throw new Exception(
                            $"Stock insuficiente para el producto {producto.nombre}.");
                    }

                    decimal subtotalDetalle =detalle.cantidad * presentacion.precio;

                    decimal descuentoDetalle = detalle.descuento;

                    if (descuentoDetalle > subtotalDetalle)
                    {
                        throw new Exception(
                            $"El descuento del producto {producto.nombre} no puede ser mayor que su subtotal.");
                    }

                    decimal totalDetalle =
                        subtotalDetalle - descuentoDetalle;

                    decimal costoDetalle =detalle.cantidad *presentacion.unidades_equivalentes *producto.costo_unitario??0;

                    decimal gananciaDetalle = totalDetalle - costoDetalle;
                    producto.stock -= unidadesADescontar;

                    var detalleVenta = new Detalle_venta
                    {
                        id_venta = venta.id_ventas,
                        id_producto = detalle.id_producto,
                        id_producto_presentacion = detalle.id_producto_presentacion,
                        cantidad = detalle.cantidad,
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

                if (venta.monto_pagado > venta.total)
                {
                    // Puedes permitirlo si manejas cambio.
                    // Aquí solo evitamos saldo negativo.
                    venta.saldo_pendiente = 0;
                    venta.id_estado_venta = 1;
                }
                else
                {
                    venta.saldo_pendiente =venta.total - venta.monto_pagado;

                    venta.id_estado_venta = venta.saldo_pendiente == 0 ? 1 : 2;
                }



              

                if (ventaDto.pago.monto_pagado > 0)
                {
                    var pago = new Pagos
                    {
                        id_venta = venta.id_ventas,
                        id_usuario = idUsuario,
                        monto = ventaDto.pago.monto_pagado,
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

        //catalodo
        public async Task<List<ProductoCatalogoDTOs>> ListarCatalogo()
        {
            var productos = await _context.productos
                .AsNoTracking()
                .Select(p => new ProductoCatalogoDTOs
                {
                    id_producto = p.id_producto,
                    nombre = p.nombre,
                    imagen = p.imagen,
                    stock = p.stock??0,

                    presentaciones = p.ProductoPresentaciones
                        .Where(pp =>
                            pp.unidades_equivalentes > 0 &&
                            pp.precio > 0)
                        .OrderBy(pp => pp.unidades_equivalentes)
                        .Select(pp => new PresentacionCatalogoDTOs
                        {
                            id_producto_presentacion =
                                pp.id_producto_presentacion,

                            presentacion = pp.descripcion,

                            unidades_equivalentes =
                                pp.unidades_equivalentes,

                            precio = pp.precio
                        })
                        .ToList()
                })
                .OrderBy(p => p.nombre)
                .ToListAsync();

            // Validación de máximo 5 presentaciones activas
            var productoConDemasiadasPresentaciones =
                productos.FirstOrDefault(p => p.presentaciones.Count > 5);

            if (productoConDemasiadasPresentaciones != null)
            {
                throw new InvalidOperationException(
                    $"El producto '{productoConDemasiadasPresentaciones.nombre}' " +
                    "tiene más de 5 presentaciones registradas."
                );
            }

            return productos;
        }


    }
}

