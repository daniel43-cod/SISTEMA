using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Data;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Cuentas;

public sealed class FiltroCuentas
{
    [RegularExpression("^(cobrar|pagar)$")] public string Tipo { get; init; } = "cobrar";
    [StringLength(100)] public string? Buscar { get; init; }
    [Range(1, 100000)] public int Pagina { get; init; } = 1;
    [Range(1, 100)] public int TamanoPagina { get; init; } = 20;
}

public sealed class CuentaItem
{
    public int Id { get; init; }
    public string Nombre { get; init; } = "";
    public string Usuario { get; init; } = "";
    public DateTime Fecha { get; init; }
    public decimal Total { get; init; }
    public decimal Saldo { get; init; }
    public decimal Subtotal { get; init; }
    public decimal? Descuento { get; init; }
    public decimal? Ganancia { get; init; }
}

public sealed record CuentasPagina(decimal TotalPorCobrar, decimal TotalPorPagar, int TotalRegistros,
    decimal SaldoFiltrado, bool HayMas, List<CuentaItem> Items);

public sealed class CuentasService(SistemaDbContext db)
{
    public async Task<CuentasPagina> Consultar(FiltroCuentas filtro, CancellationToken ct = default)
    {
        Validator.ValidateObject(filtro, new ValidationContext(filtro), true);
        // No limitar por sesión: las deudas sobreviven al cierre de caja.
        var ventas = db.ventas.AsNoTracking().Where(v => v.saldo_pendiente > 0);
        var compras = db.registroCompras.AsNoTracking().Where(c => c.SaldoPendiente > 0);
        var porCobrar = await ventas.SumAsync(v => (decimal?)v.saldo_pendiente, ct) ?? 0m;
        var porPagar = await compras.SumAsync(c => (decimal?)c.SaldoPendiente, ct) ?? 0m;
        IQueryable<CuentaItem> consulta = filtro.Tipo == "cobrar"
            ? ventas.Select(v => new CuentaItem {
                Id = v.id_ventas, Nombre = v.cliente.nombre + (v.cliente.apellido == null ? "" : " " + v.cliente.apellido),
                Usuario = v.usuario.nombre, Fecha = v.fecha_venta, Total = v.total, Saldo = v.saldo_pendiente,
                Subtotal = v.subtotal, Descuento = v.descuento, Ganancia = v.ganancia_total
            })
            : compras.Select(c => new CuentaItem {
                Id = c.IdCompra, Nombre = c.Proveedores.nombre, Usuario = c.Usuario.nombre, Fecha = c.FechaIngreso,
                Total = c.TotalCompra, Saldo = c.SaldoPendiente
            });
        if (!string.IsNullOrWhiteSpace(filtro.Buscar))
        {
            var texto = filtro.Buscar.Trim().ToLower();
            var esId = int.TryParse(texto.TrimStart('#'), out var id);
            consulta = consulta.Where(c => c.Nombre.ToLower().Contains(texto) || (esId && c.Id == id));
        }
        var count = await consulta.CountAsync(ct);
        var saldo = await consulta.SumAsync(c => (decimal?)c.Saldo, ct) ?? 0m;
        var items = await consulta.OrderByDescending(c => c.Fecha).ThenByDescending(c => c.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina).Take(filtro.TamanoPagina).ToListAsync(ct);
        return new(porCobrar, porPagar, count, saldo, (long)filtro.Pagina * filtro.TamanoPagina < count, items);
    }
}
