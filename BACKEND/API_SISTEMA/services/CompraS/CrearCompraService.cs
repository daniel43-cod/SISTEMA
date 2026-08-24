using API_SISTEMA.data;
using API_SISTEMA.DTOs.Compras;
using API_SISTEMA.models;
using API_SISTEMA.services.MovimientoCaja;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services.CompraS
{
    public class CrearCompraService
    {
        private readonly SistemaDbContext _context;
        private readonly MovimientoCajaService _movimientoCajaService;
        public CrearCompraService(SistemaDbContext context, MovimientoCajaService movimientoCajaService)
        {
            _context = context;
            _movimientoCajaService = movimientoCajaService;
        }


        public async Task<RegistroCompras> CrearCompra(RegistroComprasDTO compraDto, int idUsuario)
        {
            if (compraDto == null)
                throw new Exception("La información de la compra es obligatoria.");

            if (compraDto.detalle_compra == null || compraDto.detalle_compra.Count == 0)
            {
                throw new Exception(
                    "Debes enviar al menos un detalle de compra."
                );
            }

            bool empresaExiste = await _context.empresa
                .AnyAsync(e => e.id_empresa == compraDto.id_empresa);

            if (!empresaExiste)
                throw new Exception("La empresa indicada no existe.");

            decimal totalCompra = 0;

            foreach (var detalleDto in compraDto.detalle_compra)
            {
                if (detalleDto.cantidad <= 0)
                    throw new Exception(
                        "La cantidad debe ser mayor a cero."
                    );

                if (detalleDto.precio <= 0)
                    throw new Exception(
                        "El precio debe ser mayor a cero."
                    );

                bool productoExiste = await _context.productos
                    .AnyAsync(p =>
                        p.id_producto == detalleDto.id_producto);

                if (!productoExiste)
                {
                    throw new Exception(
                        $"El producto con ID " +
                        $"{detalleDto.id_producto} no existe."
                    );
                }

                totalCompra +=
                    detalleDto.cantidad * detalleDto.precio;
            }

            if (compraDto.monto_pagado < 0)
            {
                throw new Exception(
                    "El monto pagado no puede ser negativo."
                );
            }

            if (compraDto.monto_pagado > totalCompra)
            {
                throw new Exception(
                    "El monto pagado no puede superar " +
                    "el total de la compra."
                );
            }

            int idEstadoCompra = compraDto.monto_pagado >= totalCompra ? 1 : 2;

            using var transaccion =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var compra = new RegistroCompras
                {
                    id_usuario = idUsuario,
                    id_empresa = compraDto.id_empresa,
                    id_estado_compra = idEstadoCompra,
                    fecha_ingreso = DateTime.Now,
                    total_compra = totalCompra,
                    saldo_pendiente =
                        totalCompra - compraDto.monto_pagado
                };

                _context.registroCompras.Add(compra);

                // Necesitamos el id_compra generado.
                await _context.SaveChangesAsync();

                if (compraDto.monto_pagado > 0)
                {
                    var sesionCaja = await _context.sesioncaja
                        .FirstOrDefaultAsync(s =>
                            s.id_usuario_apertura == idUsuario &&
                            s.fecha_cierre == null
                        );

                    if (sesionCaja == null)
                    {
                        throw new Exception(
                            "Debes tener una sesión de caja abierta " +
                            "para registrar un pago."
                        );
                    }

                    var pagoCompra = new PagosCompra
                    {
                        id_compra = compra.id_compra,
                        id_usuario = idUsuario,
                        id_sesion_caja = sesionCaja.id_sesion_caja,
                        observacion =compraDto.observacion,
                        monto =compraDto.monto_pagado,
                        fecha_pago = DateTime.Now
                    };

                    _context.pagosCompras.Add(pagoCompra);

                    // Necesitamos el id del pago si lo vamos
                    // a guardar como referencia del movimiento.
                    await _context.SaveChangesAsync();

                    await _movimientoCajaService.RegistrarMovimiento(
                            idSesionCaja: sesionCaja.id_sesion_caja,
                            idUsuario: idUsuario,
                            idTipoMovimiento: TiposMovimientoCaja.PagoCompra,
                            monto: compraDto.monto_pagado,
                            descripcion: $"Pago de compra #{compra.id_compra}",
                            idCompra: compra.id_compra,
                            idPagoCompra: pagoCompra.id_pagos_compra
                        );
                }

                foreach (var detalleDto in compraDto.detalle_compra)
                {
                    var detalle = new DetalleCompra
                    {
                        id_registro_compra =compra.id_compra,
                        id_producto =detalleDto.id_producto,
                        cantidad =detalleDto.cantidad,
                        precio =detalleDto.precio
                    };

                    _context.detalle_compras.Add(detalle);

                    var producto = await _context.productos  .FirstAsync(p => p.id_producto ==detalleDto.id_producto);
                    producto.stock += detalleDto.cantidad;

                }

                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();

                return compra;
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }


    }
}
