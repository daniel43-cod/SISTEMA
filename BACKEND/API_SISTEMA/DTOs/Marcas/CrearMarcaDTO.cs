using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.DTOs.Marcas;

public sealed class CrearMarcaDTO
{
    [Required(ErrorMessage = "El nombre de la marca es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre admite hasta 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría válida.")]
    public int IdCategoria { get; set; }
}
