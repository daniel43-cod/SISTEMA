using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Dtos.Cliente;

public sealed class ActualizarClienteDto
{
    [Required, StringLength(100)] public string Nombre { get; init; } = "";
    [StringLength(100)] public string? Apellido { get; init; }
    [StringLength(20)]
    [RegularExpression("^[0-9]+$", ErrorMessage = "El NIT debe contener únicamente números o quedar vacío.")]
    public string? Nit { get; init; }
    [StringLength(20)]
    [RegularExpression("^[0-9]+$", ErrorMessage = "El DPI debe contener únicamente números o quedar vacío.")]
    public string? Dpi { get; init; }
    [StringLength(8, ErrorMessage = "El teléfono debe tener exactamente 8 dígitos o quedar vacío.")]
    [RegularExpression("^[0-9]{8}$", ErrorMessage = "El teléfono debe tener exactamente 8 dígitos o quedar vacío.")]
    public string? Telefono { get; init; }
    [StringLength(254)] public string? CorreoElectronico { get; init; }
    [StringLength(200)] public string? Direccion { get; init; }
}
