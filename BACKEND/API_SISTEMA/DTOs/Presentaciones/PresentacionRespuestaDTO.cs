namespace API_SISTEMA.DTOs.Presentaciones;

public sealed class PresentacionRespuestaDTO
{
    public int IdPresentacion { get; init; }
    public string? Descripcion { get; init; }
    public bool Estado { get; init; }
}
