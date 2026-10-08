namespace API_SISTEMA.Dtos.Presentaciones;

public sealed class PresentacionRespuestaDto
{
    public int IdPresentacion { get; init; }
    public string? Descripcion { get; init; }
    public bool Estado { get; init; }
}
