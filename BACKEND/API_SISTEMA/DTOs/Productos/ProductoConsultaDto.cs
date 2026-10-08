using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Dtos.Productos;

public sealed class ProductoConsultaDto
{
    [Range(1, 100000)] public int Pagina { get; set; } = 1;
    [Range(1, 100)] public int TamanoPagina { get; set; } = 20;
    [StringLength(100)] public string? Texto { get; set; }
    [Range(1, int.MaxValue)] public int? IdMarca { get; set; }
    [Range(1, int.MaxValue)] public int? IdCategoria { get; set; }
}








