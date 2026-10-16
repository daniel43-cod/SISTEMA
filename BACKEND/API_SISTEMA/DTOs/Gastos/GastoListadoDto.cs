namespace API_SISTEMA.Dtos.Gastos;

public sealed class GastoListadoDto
{
    public int IdGasto { get; init; }
    public string Descripcion { get; init; } = string.Empty;
    public decimal Monto { get; init; }
    public string? Observacion { get; init; }
    public int IdUsuario { get; init; }
    public string Encargado { get; init; } = string.Empty;
    public DateTime Fecha { get; init; }
}

public sealed class GastoPaginaDto
{
    public int? IdSesionCaja { get; init; }
    public int Pagina { get; init; }
    public int TamanoPagina { get; init; }
    public int Total { get; init; }
    public List<GastoListadoDto> Items { get; init; } = new();
}
