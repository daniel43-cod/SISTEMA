using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Dtos.Presentaciones;

public sealed class CrearPresentacionDto
{
    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(100, ErrorMessage = "La descripción admite hasta 100 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;
}
