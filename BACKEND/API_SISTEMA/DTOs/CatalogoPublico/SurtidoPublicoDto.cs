namespace API_SISTEMA.Dtos.CatalogoPublico;

public sealed record SurtidoPublicoDto(int IdSurtido, string Nombre, string? UrlImagen,
    IReadOnlyList<ComponenteSurtidoPublicoDto> Productos, IReadOnlyList<PresentacionSurtidoPublicoDto> Presentaciones);
public sealed record ComponenteSurtidoPublicoDto(int IdProducto, string Nombre, bool Disponible);
public sealed record PresentacionSurtidoPublicoDto(int IdProductoPresentacion, string Nombre, decimal Precio, int UnidadesEquivalentes, bool Disponible);
public sealed record SurtidosPublicosPaginaDto(int Pagina, int TamanoPagina, bool HayMas, IReadOnlyList<SurtidoPublicoDto> Surtidos);
