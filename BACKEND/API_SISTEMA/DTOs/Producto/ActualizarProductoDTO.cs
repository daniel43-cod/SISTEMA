using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_SISTEMA.DTOs.Productos;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ActualizarProductoDTO
{
    [Required, StringLength(100)]
    public string codigo_barra { get; set; } = string.Empty;
    [Required, StringLength(200)]
    public string nombre { get; set; } = string.Empty;
    [Required, Range(1, int.MaxValue)]
    public int? IdMarca { get; set; }
    [Required, Range(0, int.MaxValue)]
    public int? stock_minimo { get; set; }
}
