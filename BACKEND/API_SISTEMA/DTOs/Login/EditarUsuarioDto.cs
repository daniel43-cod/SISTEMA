using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Dtos.Login;

public sealed class EditarUsuarioDto
{
    [Range(1, int.MaxValue)] public int id_rol { get; set; }
    [Required, StringLength(100)] public required string nombre { get; set; }
    [Required, StringLength(100)] public required string apellido { get; set; }
    [Required, StringLength(50)] public required string usuario { get; set; }
    [Required, StringLength(25)] public required string telefono { get; set; }
    [EmailAddress, StringLength(254)]
    public string? corre_electronico
    {
        get;
        set => field = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
