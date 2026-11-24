using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_SISTEMA.DTOs.Surtidos;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CrearSurtidoDto
{
    [Required, MaxLength(200)] public string Nombre { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int IdMarca { get; set; }
    [Required, MinLength(1)] public List<int> IdsProductos { get; set; } = [];
    [Required, MinLength(1)] public List<CrearSurtidoPresentacionDto> Presentaciones { get; set; } = [];
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CrearSurtidoPresentacionDto
{
    [Range(1, int.MaxValue)] public int IdPresentacion { get; set; }
    [Range(1, int.MaxValue)] public int UnidadesEquivalentes { get; set; }
    public decimal Precio { get; set; }
}

public sealed record SurtidoDto(int IdSurtido, string Nombre, int IdMarca, string Marca, bool Estado,
    IReadOnlyList<SurtidoProductoDto> Productos, IReadOnlyList<SurtidoPresentacionDto> Presentaciones);
public sealed record SurtidoProductoDto(int IdProducto, string Nombre, decimal? CostoPromedio);
public sealed record SurtidoResumenDto(int IdSurtido, string Nombre, int IdMarca, string Marca, bool Estado, int Productos);
public sealed record ListaSurtidosDto(int Total, int Pagina, int Tamano, IReadOnlyList<SurtidoResumenDto> Elementos);
public sealed record SurtidoPresentacionDto(int IdProductoPresentacion, int IdPresentacion,
    string? Presentacion, int UnidadesEquivalentes, decimal Precio, decimal? PrecioMinimoActual);
