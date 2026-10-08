using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Dtos.Productos;

public sealed class ProductoDetalleDto : ProductoResumenDto
{
    public DateTime FechaCreacion { get; set; }
    public List<ProductoPresentacionDetalleDto> Presentaciones { get; set; } = [];
}
