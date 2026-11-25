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
        if (dto is null || dto.id_cliente <= 0) throw new ValidationException("Selecciona un cliente válido.");
        var lineas = dto.detalles?.Cast<CrearDetalleVentaDto>().ToArray();
        CalcularDetallesVentaService.Validar(lineas);
        VentaReglas.Texto(dto.observacion, 100, "la observación");
        var permisos = new API_SISTEMA.Services.Permisos.PermisoUsuarioService(context);
        if (!await permisos.TienePermiso(idUsuario, API_SISTEMA.Utilidades.Permisos.Vender)) throw new UnauthorizedAccessException("No tienes permiso para vender.");
        await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await CajaSesionActual.ParaOperacion(context, ct);
        var venta = await context.ventas.Include(v => v.DetalleVentas).ThenInclude(d => d.ComponentesSurtido).SingleOrDefaultAsync(v => v.id_ventas == idVenta, ct)
            ?? throw new KeyNotFoundException("La venta no existe.");
        if (await context.DocumentosFel.AnyAsync(d => d.id_venta == idVenta, ct))
            throw new VentaConflictoException("La venta tiene un documento FEL registrado y no puede modificarse mientras exista ese documento.");
        if (!await context.cliente.AnyAsync(c => c.id_cliente == dto.id_cliente, ct)) throw new ValidationException("El cliente no existe.");
        // Nunca reconstruir unidades históricas usando una presentación que pudo cambiar.
        if (venta.DetalleVentas.Any(d => d.unidades_descontadas <= 0 || d.precio <= 0 || d.costo_unitario_venta is null))
            throw new VentaConflictoException("Esta venta histórica no tiene el detalle original completo. Requiere revisión antes de modificarla.");
        var restaurar = venta.DetalleVentas.SelectMany(d => d.id_producto is int producto
            ? new[] { (Id: producto, Cantidad: d.unidades_descontadas) }
            : d.ComponentesSurtido.Select(c => (Id: c.IdProducto, Cantidad: c.Cantidad))).ToArray();
        if (venta.DetalleVentas.Any(d => d.id_surtido.HasValue &&
            (d.ComponentesSurtido.Count == 0 || d.ComponentesSurtido.Sum(c => (long)c.Cantidad) != d.unidades_descontadas)))
            throw new VentaConflictoException("La composición histórica del surtido está incompleta.");
        var ids = restaurar.Select(c => c.Id).Distinct().ToArray();
        var productos = await context.productos.Where(p => ids.Contains(p.IdProducto)).ToDictionaryAsync(p => p.IdProducto, ct);
        foreach (var grupo in restaurar.GroupBy(c => c.Id))
        {
            if (!productos.TryGetValue(grupo.Key, out var p) || p.Stock is null) throw new ValidationException("El inventario requiere revisión.");
            var stock = p.Stock.Value + grupo.Sum(c => (long)c.Cantidad);
            if (stock > int.MaxValue) throw new ValidationException("El inventario excede el límite permitido.");
            p.Stock = (int)stock;
        }
        var nuevos = await new CalcularDetallesVentaService(context).Calcular(lineas!, ct);
        var total = nuevos.Sum(d => d.subtotal);
        var receptorFiscal = ReceptorVentaReglas.ValidarOpcional(dto.tipo_identificacion_receptor, dto.identificacion_receptor, dto.nombre_receptor, total);
        var descuento = nuevos.Sum(d => d.descuento ?? 0m);
        VentaReglas.Monto(total, false); VentaReglas.Monto(total + descuento);
        var pagado = await context.pagos.Where(p => p.id_venta == idVenta).SumAsync(p => (decimal?)p.monto, ct) ?? 0m;
        if (pagado > total) throw new VentaConflictoException("El nuevo total no puede ser menor a lo ya pagado.");
        context.DetalleVentaSurtidos.RemoveRange(venta.DetalleVentas.SelectMany(d => d.ComponentesSurtido));
        context.detalle_Ventas.RemoveRange(venta.DetalleVentas);
        venta.DetalleVentas = nuevos;
        venta.id_cliente = dto.id_cliente; venta.subtotal = total + descuento; venta.descuento = descuento;
        if (receptorFiscal is not null)
        {
            venta.tipo_identificacion_receptor = receptorFiscal.Tipo;
            venta.identificacion_receptor = receptorFiscal.Identificacion;
            venta.nombre_receptor = receptorFiscal.Nombre;
        }
        venta.total = total; venta.ganancia_total = nuevos.Sum(d => d.ganancia); venta.monto_pagado = pagado;
        venta.impuesto = nuevos.Sum(d => d.impuesto ?? 0m);
        venta.saldo_pendiente = total - pagado; venta.id_estado_venta = total == pagado ? 1 : 2; venta.observacion = dto.observacion?.Trim();
        await context.SaveChangesAsync(ct); await tx.CommitAsync(ct);
    }
}
