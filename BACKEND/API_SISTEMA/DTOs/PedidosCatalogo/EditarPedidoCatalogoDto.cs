using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_SISTEMA.Dtos.PedidosCatalogo;

// Rechaza campos ajenos a los detalles, incluidos datos del cliente y totales manuales.
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EditarPedidoCatalogoDto
{
    [Required, MinLength(1), MaxLength(100)]
    public List<EditarDetallePedidoCatalogoDto> Detalles { get; init; } = new();
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EditarDetallePedidoCatalogoDto
{
    [Range(1, int.MaxValue)] public int IdProductoPresentacion { get; init; }
    [Range(1, int.MaxValue)] public int Cantidad { get; init; }
}
