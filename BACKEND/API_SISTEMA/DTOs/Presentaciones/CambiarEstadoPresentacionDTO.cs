using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_SISTEMA.DTOs.Presentaciones;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CambiarEstadoPresentacionDTO
{
    [Required] public bool? Estado { get; set; }
}
