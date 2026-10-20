using System.ComponentModel.DataAnnotations;
using System.Data;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Ventas;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Caja;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Ventas;

public class ActualizarVentaService(SistemaDbContext context)
{
    public async Task ModificarVenta(int idVenta, ModificarVentaDto dto, int idUsuario, CancellationToken ct = default)
    {
        if (dto is null || dto.id_cliente <= 0 || dto.detalles is null || dto.detalles.Count == 0 ||
            dto.detalles.Any(d => d is null || d.id_producto <= 0 || d.id_producto_presentacion <= 0 || d.cantidad <= 0))
            throw new ValidationException("Revisa el cliente y agrega al menos una línea válida a la venta.");
        if (dto.detalles.Select(d => d.id_producto_presentacion).Distinct().Count() != dto.detalles.Count)
            throw new ValidationException("No repitas la misma presentación.");
        foreach (var d in dto.detalles) VentaReglas.Monto(d.descuento);
        VentaReglas.Texto(dto.observacion, 100, "la observación");
        var permisos = new API_SISTEMA.Services.Permisos.PermisoUsuarioService(context);
        if (!await permisos.TienePermiso(idUsuario, API_SISTEMA.Utilidades.Permisos.Vender)) throw new UnauthorizedAccessException("No tienes permiso para vender.");
        await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await CajaSesionActual.ParaOperacion(context, ct);
        var venta = await context.ventas.Include(v => v.DetalleVentas).SingleOrDefaultAsync(v => v.id_ventas == idVenta, ct)
            ?? throw new KeyNotFoundException("La venta no existe.");
        if (!await context.cliente.AnyAsync(c => c.id_cliente == dto.id_cliente, ct)) throw new ValidationException("El cliente no existe.");
        // Nunca reconstruir unidades históricas usando una presentación que pudo cambiar.
        if (venta.DetalleVentas.Any(d => d.unidades_descontadas <= 0 || d.precio <= 0 || d.costo_unitario_venta is null))
            throw new VentaConflictoException("Esta venta histórica no tiene el detalle original completo. Requiere revisión antes de modificarla.");
        var ids = dto.detalles.Select(d => d.id_producto).Concat(venta.DetalleVentas.Select(d => d.id_producto)).Distinct().Order().ToArray();
        var productos = await context.productos.Include(p => p.Marca).ThenInclude(m => m.Categoria)
            .Where(p => ids.Contains(p.IdProducto)).OrderBy(p => p.IdProducto).ToDictionaryAsync(p => p.IdProducto, ct);
        foreach (var anterior in venta.DetalleVentas)
        {
            var p = productos[anterior.id_producto];
            if (p.Stock is null) throw new ValidationException("El inventario requiere revisión.");
            try { p.Stock = checked(p.Stock.Value + anterior.unidades_descontadas); }
            catch (OverflowException) { throw new ValidationException("El inventario excede el límite permitido."); }
        }
        var idsPr = dto.detalles.Select(d => d.id_producto_presentacion).ToArray();
        var presentaciones = await context.producto_presentaciones.Include(p => p.Presentacion)
            .Where(p => idsPr.Contains(p.id_producto_presentacion)).ToDictionaryAsync(p => p.id_producto_presentacion, ct);
        var nuevos = new List<DetalleVenta>();
        foreach (var d in dto.detalles)
        {
            if (!productos.TryGetValue(d.id_producto, out var p) || !p.Marca.Estado || !p.Marca.Categoria.Estado ||
                !presentaciones.TryGetValue(d.id_producto_presentacion, out var pr) || pr.id_producto != p.IdProducto || !pr.estado || pr.Presentacion.Estado != true)
                throw new ValidationException("Producto, marca, categoría y presentación deben estar activos.");
            nuevos.Add(VentaReglas.Calcular(p, pr, d.cantidad, d.descuento));
        }
        var total = nuevos.Sum(d => d.subtotal);
        var descuento = nuevos.Sum(d => d.descuento ?? 0m);
        VentaReglas.Monto(total, false); VentaReglas.Monto(total + descuento);
        var pagado = await context.pagos.Where(p => p.id_venta == idVenta).SumAsync(p => (decimal?)p.monto, ct) ?? 0m;
        if (pagado > total) throw new VentaConflictoException("El nuevo total no puede ser menor a lo ya pagado.");
        context.detalle_Ventas.RemoveRange(venta.DetalleVentas);
        venta.DetalleVentas = nuevos;
        venta.id_cliente = dto.id_cliente; venta.subtotal = total + descuento; venta.descuento = descuento;
        venta.total = total; venta.ganancia_total = nuevos.Sum(d => d.ganancia); venta.monto_pagado = pagado;
        venta.saldo_pendiente = total - pagado; venta.id_estado_venta = total == pagado ? 1 : 2; venta.observacion = dto.observacion?.Trim();
        await context.SaveChangesAsync(ct); await tx.CommitAsync(ct);
    }
}
