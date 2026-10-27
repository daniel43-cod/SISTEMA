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
            .Include(p => p.Presentacion).Include(p => p.Producto).ThenInclude(p => p.Marca).ThenInclude(m => m.Categoria)
            .Where(p => ids.Contains(p.id_producto_presentacion)).ToDictionaryAsync(p => p.id_producto_presentacion, ct);
        var unidadesPorProducto = new Dictionary<int, long>();
        var detalles = new List<DetallePedidoCatalogo>();
        decimal total = 0;
        foreach (var linea in lineas)
        {
            if (!presentaciones.TryGetValue(linea.IdProductoPresentacion, out var pr) || !pr.estado || pr.Presentacion.Estado != true ||
                !pr.Producto.Marca.Estado || !pr.Producto.Marca.Categoria.Estado || pr.unidades_equivalentes <= 0)
                throw new ValidationException("Uno de los productos o sus presentaciones ya no está disponible.");
            VentaReglas.Monto(pr.precio, false);
            if (pr.Producto.PrecioPromedioCompra is not decimal costo || costo <= 0 || pr.precio <= costo * pr.unidades_equivalentes)
                throw new ValidationException("Uno de los productos no tiene un precio de venta válido.");
            var unidades = (long)linea.Cantidad * pr.unidades_equivalentes;
            unidadesPorProducto.TryGetValue(pr.id_producto, out var anteriores);
            unidadesPorProducto[pr.id_producto] = anteriores + unidades;
            if (unidadesPorProducto[pr.id_producto] > (pr.Producto.Stock ?? 0))
                throw new ValidationException("Existencias insuficientes para las cantidades solicitadas.");
            var bruto = pr.precio * linea.Cantidad;
            VentaReglas.Monto(bruto, false);
            total += bruto;
            detalles.Add(new DetallePedidoCatalogo { IdProductoPresentacion = pr.id_producto_presentacion,
                NombreProducto = pr.Producto.Nombre, NombrePresentacion = pr.Presentacion.Descripcion,
                Cantidad = linea.Cantidad, PrecioUnitario = pr.precio });
        }
        VentaReglas.Monto(total, false);
        if (total <= minimo)
            throw new ValidationException($"El total debe ser mayor a Q{minimo:0.00} para esta zona.");
        return (detalles, total);
    }
}
