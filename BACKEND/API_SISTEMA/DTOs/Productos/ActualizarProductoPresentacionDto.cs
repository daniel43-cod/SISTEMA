using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_SISTEMA.Dtos.Productos;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ActualizarProductoPresentacionDto
{
    // null indica una nueva asociación; los IDs existentes nunca se reemplazan.
    [Range(1, int.MaxValue)] public int? id_producto_presentacion { get; set; }
    [Range(1, int.MaxValue)] public int id_presentacion { get; set; }
    [Range(1, int.MaxValue)] public int unidades_equivalentes { get; set; }
    [Range(typeof(decimal), "0.01", "9999999999999999.99")]
    public decimal precio { get; set; }
    [Required] public bool? estado { get; set; }
}
