using System.ComponentModel.DataAnnotations;
using System.Data;
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

        internal static void ValidarDto(object dto)
        {
            var errores = new List<ValidationResult>();
            if (!Validator.TryValidateObject(dto, new ValidationContext(dto), errores, true))
                throw new CompraValidationException(errores[0].ErrorMessage ?? "Datos de compra invalidos.");
        }

        internal static void ValidarMonto(decimal monto, bool permiteCero)
        {
            if (monto < (permiteCero ? 0m : 0.01m) || monto > 99999999.99m || decimal.Round(monto, 2) != monto)
                throw new CompraValidationException("Los montos deben respetar decimal(10,2), con hasta dos decimales.");
        }

        public async Task<RegistroCompras> CrearCompra( RegistroComprasDTO compraDto, int idUsuario, CancellationToken ct = default)
        {
            if (compraDto == null)
                throw new CompraValidationException(
                    "La información de la compra es obligatoria."
                );

            if (compraDto.detalle_compra == null ||
                compraDto.detalle_compra.Count == 0)
            {
                throw new CompraValidationException(
                    "Debes enviar al menos un detalle de compra."
                );
            }


            if (idUsuario <= 0) throw new CompraValidationException("El usuario no es valido.");
            ValidarDto(compraDto);
            foreach (var detalle in compraDto.detalle_compra)
            {
                if (detalle is null) throw new CompraValidationException("Hay un detalle de compra vacio.");
                ValidarDto(detalle);
                ValidarMonto(detalle.precio, false);
            }
            ValidarMonto(compraDto.monto_pagado, true);

            if (compraDto.detalle_compra.Count > 100 || compraDto.detalle_compra.Select(d => d.id_producto).Distinct().Count() != compraDto.detalle_compra.Count)
                throw new CompraValidationException("La compra admite hasta 100 productos sin repetir.");
            await using var transaccion = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            bool proveedorExiste = await _context.proveedores
                .AnyAsync(e =>
                    e.id_proveedor == compraDto.id_proveedor, ct
                );

            if (!proveedorExiste)
                throw new CompraValidationException(
                    "El proveedor indicado no existe."
                );

            // Usa el turno compartido y bloquea el cierre hasta confirmar toda la compra.
            var sesionCaja = await API_SISTEMA.services.Caja.CajaSesionActual.ParaOperacion(_context, ct);
            var idsProductos = compraDto.detalle_compra.Select(d => d.id_producto).Distinct().ToList();

            var productos = await _context.productos.Where(p =>idsProductos.Contains(p.id_producto)).ToListAsync(ct);

            var productosPorId = productos.ToDictionary(p => p.id_producto);

            decimal totalCompra = 0;

            foreach (var detalleDto in compraDto.detalle_compra)
            {
                if (detalleDto.cantidad <= 0)
                {
                    throw new CompraValidationException("La cantidad debe ser mayor a cero.");
                }

                if (detalleDto.precio <= 0)
                {
                    throw new CompraValidationException("El total del producto debe ser mayor a cero.");
                }

                if (!productosPorId.ContainsKey(detalleDto.id_producto))
                {
                    throw new CompraValidationException($"El producto con ID " +$"{detalleDto.id_producto} no existe."
                    );
                }

                // precio representa el total de esta linea, como en el contrato actual.
                totalCompra += detalleDto.precio;
                if (totalCompra > 99999999.99m) throw new CompraValidationException("El total supera el limite de decimal(10,2).");
            }

            if (compraDto.monto_pagado < 0)
            {
                throw new CompraValidationException("El monto pagado no puede ser negativo."
                );
            }

            if (compraDto.monto_pagado > totalCompra)
            {
                throw new CompraValidationException("El monto pagado no puede superar " +"el total de la compra."
                );
            }

            decimal saldoPendiente =totalCompra - compraDto.monto_pagado;

            int idEstadoCompra =saldoPendiente <= 0 ? 1 : 2;

            try
            {
                var compra = new RegistroCompras
                {
                    IdUsuario = idUsuario,
                    IdProveedor = compraDto.id_proveedor,
                    IdEstadoCompra = idEstadoCompra,
                    FechaIngreso = DateTime.Now,
                    TotalCompra = totalCompra,
                    SaldoPendiente = saldoPendiente,
                    Observacion = compraDto.observacion?.Trim()
                };

                _context.registroCompras.Add(compra);

                await _context.SaveChangesAsync(ct);

                if (compraDto.monto_pagado > 0)
                {
                    var pagoCompra = new PagosCompra
                    {
                        id_compra = compra.IdCompra,
                        id_usuario = idUsuario,
                        id_sesion_caja =sesionCaja.id_sesion_caja,
                        observacion =compraDto.observacion?.Trim() ?? string.Empty,
                        monto =compraDto.monto_pagado,
                        fecha_pago =DateTime.Now
                    };

                    _context.pagosCompras.Add(pagoCompra
                    );

                    await _context.SaveChangesAsync(ct);

                    await _movimientoCajaService.RegistrarMovimiento(
                            idSesionCaja:sesionCaja.id_sesion_caja,
                            idUsuario:idUsuario,
                            idTipoMovimiento:TiposMovimientoCaja.PagoCompra,
                            monto:compraDto.monto_pagado,
                            descripcion:$"Pago de compra #{compra.IdCompra}",
                            idCompra:compra.IdCompra,
                            idPagoCompra:pagoCompra.id_pagos_compra
                        );
                }

                foreach (var detalleDto in compraDto.detalle_compra)
                {
                    var producto =productosPorId[detalleDto.id_producto];

                    var detalle = new DetalleCompra
                    {
                        id_registro_compra =compra.IdCompra,
                        id_producto =detalleDto.id_producto,
                        cantidad =detalleDto.cantidad,
                        precio =detalleDto.precio,
                        subtotal = detalleDto.precio
                    };

                    _context.detalle_compras.Add(detalle);

                    var stockNuevo = (long)(producto.stock ?? 0) + detalleDto.cantidad;
                    if (stockNuevo > int.MaxValue)
                        throw new CompraValidationException("La cantidad supera el stock maximo permitido.");
                    producto.stock = (int)stockNuevo;
                }

                await _context.SaveChangesAsync(ct);

                await transaccion.CommitAsync(ct);

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
