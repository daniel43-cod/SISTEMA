using System.ComponentModel.DataAnnotations;
namespace API_SISTEMA.Dtos.Empresa;
public sealed class EditarEmpresaDto
{
    [Required, StringLength(150)] public string nombre_empresa { get; set; } = string.Empty;
    [Required, StringLength(20)] public string nit { get; set; } = string.Empty;
    [StringLength(150)] public string? ubicacion { get; set; }
    [StringLength(250)] public string? direccion { get; set; }
    [StringLength(150)] public string? telefonos { get; set; }
    [StringLength(250)] public string? razon_social { get; set; }
    [StringLength(250)] public string? nombre_comercial { get; set; }
    [StringLength(10)] public string? afiliacion_iva { get; set; }
    [Range(1, int.MaxValue)] public int? codigo_establecimiento { get; set; }
    [EmailAddress, StringLength(254)] public string? correo_emisor { get; set; }
    [StringLength(10)] public string? codigo_postal { get; set; }
    [StringLength(100)] public string? municipio { get; set; }
    [StringLength(100)] public string? departamento { get; set; }
    [RegularExpression("^[A-Za-z]{2}$")] public string? pais { get; set; }
}
