using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Dtos.PedidosCatalogo;

public sealed class RegistrarPedidoCatalogoDto
{
    public Guid IdOperacion { get; init; }
    [Required, StringLength(150)] public string NombreCliente { get; init; } = string.Empty;
    [Required]
    [RegularExpression(@"\A\+502[0-9]{8}\z",
        ErrorMessage = "Ingresa +502 seguido de exactamente 8 dígitos, sin espacios ni guiones.")]
    public string Telefono { get; init; } = string.Empty;
    [Required, StringLength(300)] public string Direccion { get; init; } = string.Empty;
    [StringLength(300)] public string? Referencias { get; init; }
    [StringLength(500)] public string? Observacion { get; init; }
    [Range(1, int.MaxValue)] public int IdZonaEntrega { get; init; }
    [Required, MinLength(1), MaxLength(100)]
    public List<RegistrarDetallePedidoCatalogoDto> Detalles { get; init; } = new();
}

public sealed class RegistrarDetallePedidoCatalogoDto
{
    [Range(1, int.MaxValue)] public int IdProductoPresentacion { get; init; }
    [Range(1, int.MaxValue)] public int Cantidad { get; init; }
}

public sealed record PedidoCatalogoRegistradoDto(Guid IdOperacion, decimal Total, string Estado);
public sealed record ZonaEntregaPublicaDto(int IdZonaEntrega, string Nombre, decimal MinimoCompra);
