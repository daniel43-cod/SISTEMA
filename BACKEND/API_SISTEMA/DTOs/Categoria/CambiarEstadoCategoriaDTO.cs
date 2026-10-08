using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_SISTEMA.Dtos.Categoria;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CambiarEstadoCategoriaDto
{
    [Required(ErrorMessage = "El estado es obligatorio.")]
    public bool? Estado { get; set; }
}
