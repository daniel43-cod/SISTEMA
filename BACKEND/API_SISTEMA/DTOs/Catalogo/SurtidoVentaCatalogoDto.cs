namespace API_SISTEMA.Dtos.Catalogo;

public sealed class SurtidoVentaCatalogoDto : ProductoCatalogoDto
{
    public int id_surtido { get; set; }
    public string? bloqueo { get; set; }
    public List<ComponenteSurtidoCatalogoDto> componentes { get; set; } = [];
}
public sealed record ComponenteSurtidoCatalogoDto(int id_producto, string nombre, int stock);
