using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_SISTEMA.Dtos.Productos;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class ActualizarProductoDto
{
    // Omitir conserva la imagen actual; QuitarImagen permite eliminarla.
    [StringLength(2048)]
    public string? UrlImagen { get; set; }
    public bool QuitarImagen { get; set; }
    [Required, StringLength(100)]
    public string codigo_barra { get; set; } = string.Empty;
    [Required, StringLength(200)]
    public string nombre { get; set; } = string.Empty;
    [Required, Range(1, int.MaxValue)]
    public int? IdMarca { get; set; }
    [Required, Range(0, int.MaxValue)]
    public int? stock_minimo { get; set; }
    // null conserva las presentaciones; una lista reemplaza su configuración activa.
    [MaxLength(100)]
    public List<ActualizarProductoPresentacionDto>? presentaciones { get; set; }
}
