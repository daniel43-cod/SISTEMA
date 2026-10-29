using System.Text.Json.Serialization;

namespace API_SISTEMA.Dtos.Ventas;

public sealed class VentasResumenPaginaDto
{
    public int Pagina { get; init; }
    public int TamanoPagina { get; init; }
    public int TotalRegistros { get; init; }
    public bool HayMas { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? GananciaTotal { get; init; }
    public List<ListarVentasDto> Items { get; init; } = new();
}
