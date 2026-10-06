using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_SISTEMA.DTOs.Marcas;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CambiarEstadoMarcaDTO
{
    [Required(ErrorMessage = "El estado es obligatorio.")]
    public bool? Estado { get; set; }
}
