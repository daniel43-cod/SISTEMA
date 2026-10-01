namespace API_SISTEMA.DTOs.Marcas;

public sealed class RespuestaMarcaDTO
{
    public int IdMarca { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public int IdCategoria { get; init; }
    public bool Estado { get; init; }
}
