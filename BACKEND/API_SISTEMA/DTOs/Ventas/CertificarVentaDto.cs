using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Dtos.Ventas;

public sealed class CertificarVentaDto
{
    [EmailAddress, StringLength(254)] public string? correo_receptor { get; set; }
    [Required, StringLength(250)] public string direccion { get; set; } = string.Empty;
    [Required, StringLength(10)] public string codigo_postal { get; set; } = string.Empty;
    [Required, StringLength(100)] public string municipio { get; set; } = string.Empty;
    [Required, StringLength(100)] public string departamento { get; set; } = string.Empty;
    [Required, RegularExpression("^[A-Za-z]{2}$")] public string pais { get; set; } = "GT";
    // Este flujo inicial solo admite mercadería afecta al IVA incluido del 12%.
    [Range(typeof(bool), "true", "true", ErrorMessage = "Confirma que todas las líneas son bienes afectos al IVA del 12%.")]
    public bool confirmar_bienes_gravados { get; set; }
}
