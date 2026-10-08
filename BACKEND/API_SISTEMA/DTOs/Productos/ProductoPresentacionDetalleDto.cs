using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Dtos.Productos;

public sealed class ProductoPresentacionDetalleDto
{
    public int IdProductoPresentacion { get; set; }
    public int IdPresentacion { get; set; }
    public string? Descripcion { get; set; }
    public int UnidadesEquivalentes { get; set; }
    public decimal Precio { get; set; }
    public bool Activa { get; set; }
    public bool PresentacionActiva { get; set; }
    public int PresentacionesDisponibles { get; set; }
}
