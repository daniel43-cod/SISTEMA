using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_SISTEMA.DTOs.Productos;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class ProductoPresentacionDTO
{
    [Range(1, int.MaxValue)]
    public int id_presentacion { get; set; }
    [Range(1, int.MaxValue)]
    public int unidades_equivalentes { get; set; }
    [Range(typeof(decimal), "0.01", "9999999999999999.99")]
    public decimal precio { get; set; }
}
