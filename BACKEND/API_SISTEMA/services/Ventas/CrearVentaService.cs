using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Ventas;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Caja;
using API_SISTEMA.Services.MovimientoCaja;
using API_SISTEMA.Services.Permisos;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Ventas;

public class CrearVentaService(SistemaDbContext context, MovimientoCajaService movimientos, PermisoUsuarioService permisos)
{
    public async Task<API_SISTEMA.Models.Ventas> CrearVenta(CrearVentaDto dto, int idUsuario, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (dto is null || dto.id_operacion == Guid.Empty) throw new ValidationException("Debes enviar una clave id_operacion válida para registrar la venta.");
        if (dto.detalles is null || dto.detalles.Count == 0 || dto.detalles.Any(d => d is null || d.id_producto <= 0 || d.id_producto_presentacion <= 0 || d.cantidad <= 0))
            throw new ValidationException("La venta debe contener al menos una línea con identificadores y cantidades válidos.");
        if (dto.detalles.Select(d => d.id_producto_presentacion).Distinct().Count() != dto.detalles.Count)
            throw new ValidationException("Agrupa las cantidades de una misma presentación en una sola línea.");
        foreach (var d in dto.detalles) VentaReglas.Monto(d.descuento);
        VentaReglas.Texto(dto.origen, 50, "el origen");
        VentaReglas.Texto(dto.observacion, 100, "la observación");
        if (dto.id_cliente < 0 || (dto.id_cliente == 0 && dto.clienteNuevo is null) || (dto.id_cliente > 0 && dto.clienteNuevo is not null))
            throw new ValidationException("Selecciona un cliente existente o envía un cliente nuevo, no ambos.");
        if (dto.clienteNuevo is { } c)
        {
            VentaReglas.Texto(c.nombre, 100, "el nombre del cliente", true);
            VentaReglas.Texto(c.apellido, 100, "el apellido"); VentaReglas.Texto(c.nit, 20, "el NIT");
            VentaReglas.Texto(c.dpi, 20, "el DPI"); VentaReglas.Texto(c.telefono, 30, "el teléfono");
            VentaReglas.Texto(c.correo_electronico, 254, "el correo"); VentaReglas.Texto(c.direccion, 200, "la dirección");
        }
        if (dto.pago is { } pago)
        {
            VentaReglas.Monto(pago.monto);
            if (!string.Equals(pago.metodo_pago?.Trim(), "EFECTIVO", StringComparison.OrdinalIgnoreCase))
                throw new ValidationException("El único método de pago permitido es EFECTIVO.");
            VentaReglas.Texto(pago.observacion, 100, "la observación del pago");
        }
        if (!await permisos.TienePermiso(idUsuario, API_SISTEMA.Utilidades.Permisos.Vender)) throw new UnauthorizedAccessException("No tienes permiso para vender.");
        // El hash compara el contenido de una repetición; nunca se registra otro cobro para la misma clave.
        var huella = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(dto)));
        async Task<API_SISTEMA.Models.Ventas?> Repetida()
        {
            var op = await context.Set<VentaOperacion>().AsNoTracking().SingleOrDefaultAsync(o => o.id_operacion == dto.id_operacion, ct);
            if (op is null) return null;
            if (op.id_usuario != idUsuario || op.huella != huella) throw new VentaConflictoException("La clave de operación ya se utilizó con otros datos.");
            return await context.ventas.AsNoTracking().SingleAsync(v => v.id_ventas == op.id_venta, ct);
        }
        if (await Repetida() is { } anterior) return anterior;
        // La conversión de un pedido conserva la transacción externa hasta vincularlo y auditarlo.
        await using var tx = context.Database.CurrentTransaction is null
            ? await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct) : null;
        var caja = await CajaSesionActual.ParaOperacion(context, ct);
        // Dos solicitudes con la misma clave pueden haber llegado antes de adquirir el bloqueo del turno.
        if (await Repetida() is { } repetida) return repetida;
        var productoIds = dto.detalles.Select(d => d.id_producto).Distinct().Order().ToArray();
        var productos = await context.productos.Include(p => p.Marca).ThenInclude(m => m.Categoria)
            .Where(p => productoIds.Contains(p.IdProducto)).OrderBy(p => p.IdProducto).ToDictionaryAsync(p => p.IdProducto, ct);
        var presentacionIds = dto.detalles.Select(d => d.id_producto_presentacion).ToArray();
        var presentaciones = await context.producto_presentaciones.Include(p => p.Presentacion)
            .Where(p => presentacionIds.Contains(p.id_producto_presentacion)).ToDictionaryAsync(p => p.id_producto_presentacion, ct);
        var detalles = new List<DetalleVenta>();
        foreach (var d in dto.detalles)
        {
            if (!productos.TryGetValue(d.id_producto, out var producto) || !producto.Marca.Estado || !producto.Marca.Categoria.Estado)
                throw new ValidationException("El producto, su marca y su categoría deben estar disponibles y activos.");
            if (!presentaciones.TryGetValue(d.id_producto_presentacion, out var pr) || pr.id_producto != d.id_producto || !pr.estado || pr.Presentacion.Estado != true)
                throw new ValidationException("La presentación debe pertenecer al producto y estar activa.");
            detalles.Add(VentaReglas.Calcular(producto, pr, d.cantidad, d.descuento));
        }
        var total = detalles.Sum(d => d.subtotal);
        var descuento = detalles.Sum(d => d.descuento ?? 0);
        VentaReglas.Monto(total, false); VentaReglas.Monto(total + descuento);
        var pagado = dto.pago?.monto ?? 0m;
        if (pagado > total) throw new ValidationException("El pago no puede superar el total de la venta.");
        Cliente cliente;
        if (dto.id_cliente > 0) cliente = await context.cliente.SingleOrDefaultAsync(c => c.id_cliente == dto.id_cliente, ct)
            ?? throw new ValidationException("El cliente no existe."); // El estado del cliente no bloquea la venta.
        else
        {
            var nuevo = dto.clienteNuevo!;
            cliente = new Cliente { nombre = nuevo.nombre.Trim(), apellido = nuevo.apellido?.Trim(), nit = nuevo.nit?.Trim(), dpi = nuevo.dpi?.Trim(),
                telefono = nuevo.telefono?.Trim(), correo_electronico = nuevo.correo_electronico?.Trim(), direccion = nuevo.direccion?.Trim(), estado = true };
        }
        var ahora = DateTime.Now;
        var venta = new API_SISTEMA.Models.Ventas { cliente = cliente, id_usuario = idUsuario, id_sesion_caja = caja.id_sesion_caja,
            fecha_venta = ahora, origen = dto.origen?.Trim() ?? "MOSTRADOR", observacion = dto.observacion?.Trim(),
            subtotal = total + descuento, descuento = descuento, impuesto = 0, total = total, monto_pagado = pagado,
            saldo_pendiente = total - pagado, ganancia_total = detalles.Sum(d => d.ganancia), id_estado_venta = pagado == total ? 1 : 2,
            DetalleVentas = detalles };
        context.ventas.Add(venta);
        // La FK de la operación se resuelve en el mismo SaveChanges que inserta la venta.
        context.Set<VentaOperacion>().Add(new() { id_operacion = dto.id_operacion, id_usuario = idUsuario, huella = huella, Venta = venta });
        Pagos? registroPago = null;
        if (pagado > 0)
        {
            registroPago = new Pagos { id_usuario = idUsuario, id_sesion_caja = caja.id_sesion_caja, monto = pagado,
                metodo_pago = "EFECTIVO", observacion = dto.pago?.observacion?.Trim(), fecha_pago = ahora };
        }
        await context.SaveChangesAsync(ct);
        if (registroPago is not null)
        {
            registroPago.id_venta = venta.id_ventas; context.pagos.Add(registroPago); await context.SaveChangesAsync(ct);
            await movimientos.RegistrarMovimiento(caja.id_sesion_caja, idUsuario, TiposMovimientoCaja.Venta, pagado,
                $"Pago de venta #{venta.id_ventas}", idVenta: venta.id_ventas, idPagoVenta: registroPago.id_pago, cancellationToken: ct);
            await context.SaveChangesAsync(ct);
        }
        if (tx is not null) await tx.CommitAsync(ct);
        return venta;
    }
}
