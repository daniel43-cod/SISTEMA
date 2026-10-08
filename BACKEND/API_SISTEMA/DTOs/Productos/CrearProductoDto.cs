using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_SISTEMA.Dtos.Productos;

// Solo datos que el usuario puede elegir; stock, costo y fecha los asigna la API.
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class CrearProductoDto
{
    [Required, StringLength(100)]
    public string codigo_barra { get; set; } = string.Empty;
    [Required, StringLength(200)]
    public string nombre { get; set; } = string.Empty;
    [Range(1, int.MaxValue)]
    public int IdMarca { get; set; }
    [Range(0, int.MaxValue)]
    public int stock_minimo { get; set; }
    [StringLength(2048)]
    public string? UrlImagen { get; set; }
    [Required, MinLength(1), MaxLength(100)]
    public List<ProductoPresentacionDto> presentaciones { get; set; } = new();
}
