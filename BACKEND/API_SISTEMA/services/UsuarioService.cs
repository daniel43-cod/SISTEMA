using API_SISTEMA.Dtos.Login;
using API_SISTEMA.Services.Usuarios;

namespace API_SISTEMA.Services;

public class UsuarioService(UsuarioListarService listar, UsuarioCrearService crear)
{
    public Task<List<UsuarioRespuestaDto>> ListarUsuario(CancellationToken cancellationToken = default) =>
        listar.ListarUsuario(cancellationToken);

    public Task<UsuarioRespuestaDto> CrearUsuario(CrearCuentaDto dto, CancellationToken cancellationToken = default)
        => crear.CrearUsuario(dto, cancellationToken);
}
