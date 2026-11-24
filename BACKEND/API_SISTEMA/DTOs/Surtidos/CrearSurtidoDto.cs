using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_SISTEMA.DTOs.Surtidos;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class CrearSurtidoDto
{
    [Required, MaxLength(200)] public string Nombre { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int IdMarca { get; set; }
    [MaxLength(2048)] public string? UrlImagen { get; set; }
    [Required, MinLength(1)] public List<int> IdsProductos { get; set; } = [];
    [Required, MinLength(1)] public List<CrearSurtidoPresentacionDto> Presentaciones { get; set; } = [];
}

public sealed class CrearSurtidoConImagenDto : CrearSurtidoDto
{
    public IFormFile? Imagen { get; set; }
}

public class ActualizarSurtidoDto : CrearSurtidoDto
{
    public bool QuitarImagen { get; set; }
    public bool Estado { get; set; } = true;
}

public sealed class ActualizarSurtidoConImagenDto : ActualizarSurtidoDto
{
    public IFormFile? Imagen { get; set; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CrearSurtidoPresentacionDto
{
    [Range(1, int.MaxValue)] public int IdPresentacion { get; set; }
    [Range(1, int.MaxValue)] public int UnidadesEquivalentes { get; set; }
    public decimal Precio { get; set; }
}

public sealed record SurtidoDto(int IdSurtido, string Nombre, int IdMarca, string Marca, bool Estado,
    IReadOnlyList<SurtidoProductoDto> Productos, IReadOnlyList<SurtidoPresentacionDto> Presentaciones, string? Imagen);
public sealed record SurtidoProductoDto(int IdProducto, string Nombre, decimal? CostoPromedio);
public sealed record SurtidoTarjetaPresentacionDto(string? Nombre, int UnidadesEquivalentes, decimal Precio);
public sealed record SurtidoResumenDto(int IdSurtido, string Nombre, int IdMarca, string Marca, bool Estado, int Productos, string? Imagen,
    IReadOnlyList<SurtidoTarjetaPresentacionDto> Presentaciones);
public sealed record ListaSurtidosDto(int Total, int Pagina, int Tamano, IReadOnlyList<SurtidoResumenDto> Elementos);
public sealed record SurtidoPresentacionDto(int IdProductoPresentacion, int IdPresentacion,
    string? Presentacion, int UnidadesEquivalentes, decimal Precio, decimal? PrecioMinimoActual);
