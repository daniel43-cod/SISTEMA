using System.Text.Json.Serialization;

namespace API_SISTEMA.Dtos.Ventas;

public sealed class VentasResumenPaginaDto
{
    public int Pagina { get; init; }
    public int TamanoPagina { get; init; }
    public int TotalRegistros { get; init; }
    public bool HayMas { get; init; }
    public decimal TotalPorCobrar { get; init; }
    public int CantidadPendientes { get; init; }
    public int CantidadPagadas { get; init; }
    public List<ClienteVentasCajaDto> ClientesCajaActiva { get; init; } = new();
    public decimal TotalVentas { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? GananciaTotal { get; init; }
    public List<ListarVentasDto> Items { get; init; } = new();
}

public sealed class ClienteVentasCajaDto
{
    public int IdCliente { get; init; }
    public string Nombre { get; init; } = string.Empty;
}