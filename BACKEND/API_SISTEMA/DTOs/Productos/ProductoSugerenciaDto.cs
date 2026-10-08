using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Dtos.Productos;

public sealed class ProductoSugerenciaDto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
