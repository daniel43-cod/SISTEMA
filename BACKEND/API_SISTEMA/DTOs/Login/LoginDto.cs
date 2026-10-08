using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Utilidades;

namespace API_SISTEMA.Dtos.Login;

public class LoginDto
{
    [Required, StringLength(50)]
    public string usuario { get; set; } = "";
    [Required, BCryptPassword]
    public string password { get; set; } = "";
}