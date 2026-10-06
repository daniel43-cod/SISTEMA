using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.DTOs.Marcas;

public class ActualizarMarcaDTO
{
    [Required(ErrorMessage = "El nombre de la marca es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre admite hasta 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría válida.")]
    public int IdCategoria { get; set; }

    [StringLength(2048)]
    public string? UrlImagen { get; set; }
    public bool QuitarImagen { get; set; }
}

