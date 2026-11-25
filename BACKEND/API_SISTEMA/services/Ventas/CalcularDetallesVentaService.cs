using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Ventas;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Surtidos;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Ventas;

// Se ejecuta dentro de la transacción de venta. No altera existencias hasta validar todas las líneas.
public sealed class CalcularDetallesVentaService(SistemaDbContext db)
{
    public static void Validar(IReadOnlyList<CrearDetalleVentaDto>? detalles)
    {
        if (detalles is null || detalles.Count == 0 || detalles.Any(d => d is null))
            throw new ValidationException("Agrega al menos una línea válida.");
        foreach (var d in detalles)
        {
            if (d.cantidad <= 0 || d.id_producto_presentacion <= 0 ||
                !((d.id_producto is > 0 && d.id_surtido is null) || (d.id_surtido is > 0 && d.id_producto is null)))
                throw new ValidationException("Cada línea debe identificar un producto o un surtido, nunca ambos.");
            VentaReglas.Monto(d.descuento);
            if (d.id_surtido.HasValue)
            {
                if (d.componentes is null || d.componentes.Count == 0 || d.componentes.Any(c => c is null || c.id_producto <= 0 || c.cantidad <= 0) ||
                    d.componentes.Select(c => c.id_producto).Distinct().Count() != d.componentes.Count)
                    throw new ValidationException("Los componentes deben tener cantidades enteras positivas y no repetirse.");
            }
            else if (d.componentes?.Count > 0) throw new ValidationException("Un producto individual no admite componentes de surtido.");
        }
        if (detalles.Select(d => d.id_producto_presentacion).Distinct().Count() != detalles.Count)
            throw new ValidationException("Agrupa las cantidades de una misma presentación en una sola línea.");
    }

    public async Task<List<DetalleVenta>> Calcular(IReadOnlyList<CrearDetalleVentaDto> lineas, CancellationToken ct, bool descontarInventario = true)
    {
        Validar(lineas);
        var idsPr = lineas.Select(d => d.id_producto_presentacion).ToArray();
        var presentaciones = await db.producto_presentaciones.Include(p => p.Presentacion)
            .Where(p => idsPr.Contains(p.id_producto_presentacion)).ToDictionaryAsync(p => p.id_producto_presentacion, ct);
        var idsSurtidos = lineas.Where(d => d.id_surtido.HasValue).Select(d => d.id_surtido!.Value).Distinct().ToArray();
        var surtidos = idsSurtidos.Length == 0 ? new Dictionary<int, Surtido>()
            : await db.Surtidos.Include(s => s.Marca).ThenInclude(m => m.Categoria).Include(s => s.Productos)
                .Where(s => idsSurtidos.Contains(s.IdSurtido)).ToDictionaryAsync(s => s.IdSurtido, ct);
        var ids = lineas.Where(d => d.id_producto.HasValue).Select(d => d.id_producto!.Value)
            .Concat(surtidos.Values.SelectMany(s => s.Productos.Select(p => p.IdProducto))).Distinct().Order().ToArray();
        var productos = await db.productos.Include(p => p.Marca).ThenInclude(m => m.Categoria)
            .Where(p => ids.Contains(p.IdProducto)).OrderBy(p => p.IdProducto).ToDictionaryAsync(p => p.IdProducto, ct);
        var usados = new Dictionary<int, long>();
        void Usar(int id, int cantidad) => usados[id] = usados.GetValueOrDefault(id) + cantidad;
        var resultado = new List<DetalleVenta>();
        foreach (var d in lineas)
        {
            if (!presentaciones.TryGetValue(d.id_producto_presentacion, out var pr) || !pr.estado || pr.Presentacion.Estado != true)
                throw new ValidationException("La presentación debe existir y estar activa.");
            if (d.id_producto is int idProducto)
            {
                if (pr.id_producto != idProducto || pr.id_surtido is not null || !productos.TryGetValue(idProducto, out var producto) || !Activo(producto))
                    throw new ValidationException("Producto, marca, categoría y presentación deben estar activos.");
                var copia = new Models.Productos { IdProducto = producto.IdProducto, Nombre = producto.Nombre,
                    Stock = producto.Stock, PrecioPromedioCompra = producto.PrecioPromedioCompra };
                var detalle = VentaReglas.Calcular(copia, pr, d.cantidad, d.descuento);
                Usar(idProducto, detalle.unidades_descontadas); resultado.Add(detalle);
                continue;
            }
            if (!surtidos.TryGetValue(d.id_surtido!.Value, out var surtido) || !surtido.Estado || !surtido.Marca.Estado || !surtido.Marca.Categoria.Estado ||
                pr.id_surtido != surtido.IdSurtido || pr.id_producto is not null)
                throw new ValidationException("El surtido, su marca, categoría y presentación deben estar activos.");
            var permitidos = surtido.Productos.Select(p => productos[p.IdProducto]).ToArray();
            if (permitidos.Any(p => !Activo(p))) throw new ValidationException("Hay productos del surtido con marca o categoría inactiva.");
            SurtidoReglas.ValidarCostosPromedio(permitidos);
            int unidades;
            try { unidades = checked(pr.unidades_equivalentes * d.cantidad); }
            catch (OverflowException) { throw new ValidationException("La cantidad del surtido excede el límite permitido."); }
            if (pr.unidades_equivalentes <= 0 || d.componentes!.Sum(c => (long)c.cantidad) != unidades)
                throw new ValidationException("Completa exactamente las unidades de la presentación del surtido.");
            var permitidosIds = permitidos.Select(p => p.IdProducto).ToHashSet();
            if (d.componentes.Any(c => !permitidosIds.Contains(c.id_producto))) throw new ValidationException("Un componente no está permitido en este surtido.");
            var costo = d.componentes.Sum(c => productos[c.id_producto].PrecioPromedioCompra!.Value * c.cantidad);
            VentaReglas.Monto(pr.precio, false);
            var bruto = pr.precio * d.cantidad; VentaReglas.Monto(bruto, false);
            var neto = bruto - d.descuento; VentaReglas.Monto(neto, false);
            SurtidoReglas.ValidarImporteVenta(neto, costo);
            var linea = new DetalleVenta { id_surtido = surtido.IdSurtido, id_producto_presentacion = pr.id_producto_presentacion,
                cantidad = d.cantidad, precio = pr.precio, descuento = d.descuento, subtotal = neto, unidades_descontadas = unidades,
                costo_unitario_venta = decimal.Round(costo / unidades, 6), ganancia = neto - costo, impuesto = IvaIncluido.Calcular(neto).Impuesto };
            foreach (var c in d.componentes)
            {
                Usar(c.id_producto, c.cantidad);
                linea.ComponentesSurtido.Add(new DetalleVentaSurtido { IdProducto = c.id_producto, Cantidad = c.cantidad,
                    CostoUnitarioVenta = productos[c.id_producto].PrecioPromedioCompra!.Value });
            }
            resultado.Add(linea);
        }
        foreach (var (id, cantidad) in usados)
            if (cantidad > int.MaxValue || productos[id].Stock is not int stock || stock < cantidad)
                throw new ValidationException($"Existencias insuficientes para {productos[id].Nombre}.");
        if (descontarInventario) foreach (var (id, cantidad) in usados) productos[id].Stock -= (int)cantidad;
        return resultado;
    }

    private static bool Activo(Models.Productos p) => p.Marca.Estado && p.Marca.Categoria.Estado;
}
