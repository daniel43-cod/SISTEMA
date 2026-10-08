using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Dtos.Productos;

public class ProductoResumenDto
{
    public int IdProducto { get; set; }
    public string? CodigoBarra { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Imagen { get; set; }
    public int StockUnidades { get; set; }
    public int StockMinimo { get; set; }
    public int IdMarca { get; set; }
    public string Marca { get; set; } = string.Empty;
    public bool MarcaActiva { get; set; }
    public int IdCategoria { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public bool CategoriaActiva { get; set; }
}
