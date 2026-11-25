using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.PedidosCatalogo;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Ventas;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.PedidosCatalogo;

internal static class CalcularDetallesPedido
{
    public static async Task<(List<DetallePedidoCatalogo> Detalles, decimal Total)> Calcular(
        SistemaDbContext context, IReadOnlyList<EditarDetallePedidoCatalogoDto> lineas, decimal minimo, CancellationToken ct)
    {
        if (lineas is null || lineas.Count is < 1 or > 100 || lineas.Any(d => d is null || d.IdProductoPresentacion <= 0 || d.Cantidad <= 0))
            throw new ValidationException("Envía entre 1 y 100 presentaciones con cantidades mayores a cero.");
        if (lineas.Select(d => d.IdProductoPresentacion).Distinct().Count() != lineas.Count)
            throw new ValidationException("Agrupa cada presentación en una sola línea.");
        var ids = lineas.Select(d => d.IdProductoPresentacion).ToArray();
        var presentaciones = await context.producto_presentaciones.AsNoTracking()
            .Include(p => p.Surtido).Include(p => p.Presentacion).Include(p => p.Producto).ThenInclude(p => p.Marca).ThenInclude(m => m.Categoria)
            .Where(p => ids.Contains(p.id_producto_presentacion)).ToDictionaryAsync(p => p.id_producto_presentacion, ct);
        var solicitudes = new List<API_SISTEMA.Dtos.Ventas.CrearDetalleVentaDto>();
        foreach (var linea in lineas)
        {
            if (!presentaciones.TryGetValue(linea.IdProductoPresentacion, out var pr))
                throw new ValidationException("Una presentación ya no está disponible.");
            solicitudes.Add(new()
            {
                id_producto = pr.id_producto, id_surtido = pr.id_surtido,
                id_producto_presentacion = pr.id_producto_presentacion, cantidad = linea.Cantidad,
                componentes = linea.Componentes?.Select(c => c is null ? null! : new API_SISTEMA.Dtos.Ventas.ComponenteSurtidoVentaDto
                    { id_producto = c.IdProducto, cantidad = c.Cantidad }).ToList()
            });
        }
        var calculados = await new CalcularDetallesVentaService(context).Calcular(solicitudes, ct, descontarInventario: false);
        var idsComponentes = calculados.SelectMany(d => d.ComponentesSurtido.Select(c => c.IdProducto)).Distinct().ToArray();
        var nombres = await context.productos.AsNoTracking().Where(p => idsComponentes.Contains(p.IdProducto))
            .ToDictionaryAsync(p => p.IdProducto, p => p.Nombre, ct);
        var detalles = calculados.Select(d =>
        {
            var pr = presentaciones[d.id_producto_presentacion];
            return new DetallePedidoCatalogo
            {
                IdProductoPresentacion = pr.id_producto_presentacion,
                NombreProducto = pr.id_surtido.HasValue ? pr.Surtido!.Nombre : pr.Producto!.Nombre,
                NombrePresentacion = pr.Presentacion.Descripcion ?? string.Empty,
                Cantidad = d.cantidad, PrecioUnitario = d.precio,
                Componentes = d.ComponentesSurtido.Select(c => new DetallePedidoSurtido { IdProducto = c.IdProducto, Cantidad = c.Cantidad, NombreProducto = nombres[c.IdProducto] }).ToList()
            };
        }).ToList();
        var total = calculados.Sum(d => d.subtotal);
        VentaReglas.Monto(total, false);
        if (total <= minimo)
            throw new ValidationException($"El total debe ser mayor a Q{minimo:0.00} para esta zona.");
        return (detalles, total);
    }
}
