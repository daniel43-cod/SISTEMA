namespace API_SISTEMA.Dtos.Login;

public sealed class UsuarioRespuestaDto
{
    public int id_usuario { get; init; }
    public int id_rol { get; init; }
    public string nombre { get; init; } = "";
    public string apellido { get; init; } = "";
    public string usuario { get; init; } = "";
    public string? correo { get; init; }
    public string telefono { get; init; } = "";
    public bool estado { get; init; }
    public DateTime fecha_Creacion { get; init; }
}