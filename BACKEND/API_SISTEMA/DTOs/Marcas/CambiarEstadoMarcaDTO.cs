using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_SISTEMA.Dtos.Marcas;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CambiarEstadoMarcaDto
{
    [Required(ErrorMessage = "El estado es obligatorio.")]
    public bool? Estado { get; set; }
}
