using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_SISTEMA.Dtos.PedidosCatalogo;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ConfirmarVentaPedidoDto
{
    public Guid IdOperacion { get; init; }
    [Range(typeof(decimal), "0.01", "99999999.99")] public decimal TotalEsperado { get; init; }
    [Range(typeof(decimal), "0", "99999999.99")] public decimal PagoInicial { get; init; }
    [Required, MinLength(1), MaxLength(100)] public List<ConfirmarLineaPedidoDto> Detalles { get; init; } = new();
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ConfirmarLineaPedidoDto
{
    [Range(1, int.MaxValue)] public int IdProductoPresentacion { get; init; }
    [Range(1, int.MaxValue)] public int Cantidad { get; init; }
    [Range(typeof(decimal), "0", "99999999.99")] public decimal Descuento { get; init; }
}
