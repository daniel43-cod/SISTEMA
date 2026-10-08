using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Dtos.Productos;

public sealed class ProductoPaginaDto
{
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int Total { get; set; }
    public List<ProductoResumenDto> Items { get; set; } = [];
}
