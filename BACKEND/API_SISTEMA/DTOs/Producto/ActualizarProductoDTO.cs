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
    // null conserva las presentaciones; una lista reemplaza su configuración activa.
    [MaxLength(100)]
    public List<ActualizarProductoPresentacionDTO>? presentaciones { get; set; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ActualizarProductoPresentacionDTO
{
    // null indica una nueva asociación; los IDs existentes nunca se reemplazan.
    [Range(1, int.MaxValue)] public int? id_producto_presentacion { get; set; }
    [Range(1, int.MaxValue)] public int id_presentacion { get; set; }
    [Range(1, int.MaxValue)] public int unidades_equivalentes { get; set; }
    [Range(typeof(decimal), "0.01", "9999999999999999.99")]
    public decimal precio { get; set; }
    [Required] public bool? estado { get; set; }
}
