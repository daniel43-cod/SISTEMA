using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.DTOs.Presentaciones;

public sealed class ActualizarPresentacionDTO
{
    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(100, ErrorMessage = "La descripción admite hasta 100 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;
}
