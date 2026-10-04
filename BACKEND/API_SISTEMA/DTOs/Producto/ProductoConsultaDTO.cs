using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.DTOs.Productos;

public sealed class ProductoConsultaDTO
{
    [Range(1, 100000)] public int Pagina { get; set; } = 1;
    [Range(1, 100)] public int TamanoPagina { get; set; } = 20;
    [StringLength(100)] public string? Texto { get; set; }
    [Range(1, int.MaxValue)] public int? IdMarca { get; set; }
    [Range(1, int.MaxValue)] public int? IdCategoria { get; set; }
}

public sealed class ProductoPaginaDTO
{
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int Total { get; set; }
    public List<ProductoResumenDTO> Items { get; set; } = [];
}

public class ProductoResumenDTO
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

public sealed class ProductoDetalleDTO : ProductoResumenDTO
{
    public DateTime FechaCreacion { get; set; }
    public List<ProductoPresentacionDetalleDTO> Presentaciones { get; set; } = [];
}

public sealed class ProductoPresentacionDetalleDTO
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
