using System.ComponentModel.DataAnnotations;
using System.Data;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Compras;
using API_SISTEMA.Models;
using API_SISTEMA.Services.MovimientoCaja;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Compras
{
    public class CrearCompraService
    {
        private readonly SistemaDbContext _context;
        private readonly TimeProvider _reloj;
        private readonly MovimientoCajaService _movimientoCajaService;
        public CrearCompraService(SistemaDbContext context, MovimientoCajaService movimientoCajaService, TimeProvider? reloj = null)
        {
            _context = context;
            _reloj = reloj ?? TimeProvider.System;
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

        public async Task<RegistroCompras> CrearCompra( RegistroComprasDto compraDto, int idUsuario, CancellationToken ct = default)
        {
            if (compraDto == null)throw new CompraValidationException("La información de la compra es obligatoria.");

            if (compraDto.detalle_compra == null || compraDto.detalle_compra.Count == 0)
            {
                throw new CompraValidationException("Debes enviar al menos un detalle de compra."
);
            }


            if (idUsuario <= 0) throw new CompraValidationException("El usuario no es valido.");
            ValidarDto(compraDto);
            if (compraDto.id_operacion == Guid.Empty) throw new CompraValidationException("Debes enviar una clave id_operacion válida para registrar la compra.");
            foreach (var detalle in compraDto.detalle_compra)
            {
                if (detalle is null) throw new CompraValidationException("Hay un detalle de compra vacio.");
                ValidarDto(detalle);
                ValidarMonto(detalle.precio, false);
            }
            ValidarMonto(compraDto.monto_pagado, true);

            if (compraDto.detalle_compra.Count > 100 || compraDto.detalle_compra.Select(d => d.id_producto).Distinct().Count() != compraDto.detalle_compra.Count)
                throw new CompraValidationException("La compra admite hasta 100 productos sin repetir.");
            var huella = HuellaOperacion.Calcular(new { compraDto.id_proveedor,
                nombre = compraDto.nombre_proveedor?.Trim(), observacion = compraDto.observacion?.Trim() ?? "",
                compraDto.monto_pagado, detalles = compraDto.detalle_compra.OrderBy(d => d.id_producto).Select(d => new { d.id_producto, d.cantidad, d.precio }).ToArray() });
            async Task<RegistroCompras?> Repetida()
            {
                var anterior = await _context.registroCompras.AsNoTracking().SingleOrDefaultAsync(c => c.IdOperacion == compraDto.id_operacion, ct);
                if (anterior is not null && (anterior.IdUsuario != idUsuario || anterior.HuellaOperacion != huella))
                    throw new CompraOperacionConflictException();
                return anterior;
            }
            if (await Repetida() is { } original) return original;
            await using var transaccion = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            // Usa el turno compartido y bloquea el cierre hasta confirmar toda la compra.
            var sesionCaja = await API_SISTEMA.Services.Caja.CajaSesionActual.ParaOperacion(_context, ct);
            if (await Repetida() is { } repetida) return repetida;
            // La consulta y el alta comparten la transacción serializable de la compra.
            // Dos altas concurrentes no pueden confirmar el mismo nombre como nuevo.
            Proveedores? proveedor;
            if (compraDto.id_proveedor is int idProveedor)
            {
                proveedor = await _context.proveedores.SingleOrDefaultAsync(p => p.id_proveedor == idProveedor, ct)
                    ?? throw new CompraValidationException("El proveedor indicado no existe.");
            }
            else
            {
                var nombre = compraDto.nombre_proveedor!.Trim();
                // Ambas cadenas se comparan en la base: no mezclar ToUpperInvariant de
                // .NET con upper de PostgreSQL, que pueden diferir para nombres con tildes.
                var coincidencias = _context.Database.IsNpgsql()
                    ? _context.proveedores.FromSqlInterpolated($"SELECT * FROM public.proveedores WHERE upper(trim(nombre) COLLATE pg_unicode_fast) = upper({nombre} COLLATE pg_unicode_fast)")
                    : _context.proveedores.FromSqlInterpolated($"SELECT * FROM proveedores WHERE upper(trim(nombre)) = upper({nombre})");
                proveedor = await coincidencias.OrderBy(p => p.id_proveedor).FirstOrDefaultAsync(ct);
                proveedor ??= new Proveedores { nombre = nombre, telefono = string.Empty, fecha_creacion = RelojNegocio.Ahora(_reloj) };
            }
            var idsProductos = compraDto.detalle_compra.Select(d => d.id_producto).Distinct().ToList();

            var productos = await _context.productos.Where(p =>idsProductos.Contains(p.IdProducto)).ToListAsync(ct);

            var productosPorId = productos.ToDictionary(p => p.IdProducto);

            decimal totalCompra = 0;
            var inventarioActualizado = new Dictionary<int, (int Stock, decimal CostoPromedio)>();

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

                var producto = productosPorId[detalleDto.id_producto];
                var stockAnterior = producto.Stock ?? 0;
                var costoAnterior = producto.PrecioPromedioCompra ?? 0m;
                if (stockAnterior < 0 || costoAnterior < 0)
                    throw new CompraValidationException($"El producto {producto.Nombre} tiene inventario o costo inválido.");
                var stockNuevo = (long)stockAnterior + detalleDto.cantidad;
                if (stockNuevo > int.MaxValue)
                    throw new CompraValidationException("La cantidad supera el stock maximo permitido.");

                // precio es el total de la línea; cantidad son unidades base, no paquetes.
                // Si no hay costo previo, la nueva compra sirve como referencia para todo el stock.
                var costoPromedio = decimal.Round(
                    costoAnterior == 0m
                        ? detalleDto.precio / detalleDto.cantidad
                        : (stockAnterior * costoAnterior + detalleDto.precio) / stockNuevo,
                    6, MidpointRounding.AwayFromZero);
                if (costoPromedio <= 0m || costoPromedio > 999999999999.999999m)
                    throw new CompraValidationException("El precio promedio de compra supera la precisión permitida.");
                inventarioActualizado.Add(producto.IdProducto, ((int)stockNuevo, costoPromedio));

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
                if (proveedor.id_proveedor == 0)
                {
                    _context.proveedores.Add(proveedor);
                    await _context.SaveChangesAsync(ct);
                }
                var ahora = RelojNegocio.Ahora(_reloj);
                var compra = new RegistroCompras
                {
                    IdOperacion = compraDto.id_operacion, HuellaOperacion = huella,
                    IdUsuario = idUsuario,
                    IdSesionCaja = sesionCaja.id_sesion_caja,
                    IdProveedor = proveedor.id_proveedor,
                    IdEstadoCompra = idEstadoCompra,
                    FechaIngreso = ahora,
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
                        fecha_pago = ahora
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

                    var inventario = inventarioActualizado[producto.IdProducto];
                    producto.PrecioPromedioCompra = inventario.CostoPromedio;
                    producto.Stock = inventario.Stock;
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
