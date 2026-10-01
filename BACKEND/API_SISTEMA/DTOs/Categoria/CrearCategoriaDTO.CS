using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.DTOs.Categoria;

public class CrearCategoriaDTO
{
    // El servidor asigna el estado y la fecha.
    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre admite hasta 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(2048)]
    public string? UrlImagen { get; set; }
}

