namespace API_SISTEMA.Dtos.PedidosCatalogo;

public sealed class PedidosPendientesPaginaDto
{
    public int Pagina { get; init; }
    public int TamanoPagina { get; init; } = 50;
    public bool HayMas { get; init; }
    public List<PedidoPendienteDto> Pedidos { get; init; } = new();
}

public sealed class PedidoPendienteDto
{
    public int IdPedido { get; init; }
    public DateTime FechaUtc { get; init; }
    public string NombreCliente { get; init; } = string.Empty;
    public string Telefono { get; init; } = string.Empty;
    public string Direccion { get; init; } = string.Empty;
    public string? Referencias { get; init; }
    public string? Observacion { get; init; }
    public int IdZonaEntrega { get; init; }
    public string ZonaEntrega { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public string Estado { get; init; } = "Pendiente";
}
