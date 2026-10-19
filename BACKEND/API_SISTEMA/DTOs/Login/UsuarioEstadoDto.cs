using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Dtos.Login;

public sealed class UsuarioEstadoDto
{
    [Required] public bool? estado { get; set; }
}
