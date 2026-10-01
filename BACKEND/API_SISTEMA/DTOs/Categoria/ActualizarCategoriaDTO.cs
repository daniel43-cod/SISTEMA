using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.DTOs.Categoria;

public class ActualizarCategoriaDTO
{
    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre admite hasta 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    // Sin enlace ni archivo, se conserva la imagen actual.
    [StringLength(2048)]
    public string? UrlImagen { get; set; }

    // Quitar una imagen debe ser una decisión explícita.
    public bool QuitarImagen { get; set; }
}
