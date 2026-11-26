using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Dtos.Cliente;

public class PaginaClientesFiltro
{
    [Range(1, 100000)] public int Pagina { get; init; } = 1;
    [Range(1, 100)] public int TamanoPagina { get; init; } = 30;
}

public sealed class ListadoClientesFiltro : PaginaClientesFiltro
{
    [StringLength(100)] public string? Buscar { get; init; }
}

public sealed class HistorialClientesFiltro : PaginaClientesFiltro
{
    public DateOnly? Fecha { get; init; }
}

public sealed record ClienteListadoItem(int IdCliente, string Nombre, string? Nit, string? Telefono, bool Estado, string? Direccion);
public sealed record ClientesPagina(int TotalRegistros, int Pagina, int TamanoPagina, bool HayMas, List<ClienteListadoItem> Items);
public sealed record ClienteResumen(ClienteDto Cliente, bool Estado, int CantidadVentas, decimal TotalComprado, decimal TotalPagado, decimal SaldoPendiente);
public sealed record ClienteVentaItem(int IdVenta, DateTime Fecha, int IdUsuario, string Responsable, decimal Total, decimal Pagado, decimal Saldo, int IdEstadoVenta, string Estado, string NombreResponsable);
public sealed record ClienteVentasPagina(int IdCliente, int TotalRegistros, int Pagina, int TamanoPagina, bool HayMas, List<ClienteVentaItem> Items);
