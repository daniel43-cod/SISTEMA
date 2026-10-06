using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_SISTEMA.DTOs.Categoria;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CambiarEstadoCategoriaDTO
{
    [Required(ErrorMessage = "El estado es obligatorio.")]
    public bool? Estado { get; set; }
}
