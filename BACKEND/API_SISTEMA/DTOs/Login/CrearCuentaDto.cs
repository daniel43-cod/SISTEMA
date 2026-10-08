using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Utilidades;

namespace API_SISTEMA.Dtos.Login;

public class CrearCuentaDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El rol del usuario es obligatorio.")]
    public int id_rol { get; set; }
    [Required, StringLength(100)]
    public required string nombre { get; set; }
    [Required, StringLength(100)]
    public required string apellido { get; set; }
    [Required, StringLength(50)]
    public required string usuario { get; set; }
    [EmailAddress, StringLength(254)]
    public string? corre_electronico { get; set; }
    [Required, StringLength(25)]
    public required string telefono { get; set; }
    [Required, MinLength(8), BCryptPassword]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])[\s\S]+$",
        ErrorMessage = "La contraseña debe incluir minúscula, mayúscula y número.")]
    public required string password { get; set; }
}