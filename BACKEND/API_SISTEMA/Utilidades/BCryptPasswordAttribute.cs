using System.ComponentModel.DataAnnotations;
using System.Text;

namespace API_SISTEMA.Utilidades;

public sealed class BCryptPasswordAttribute : ValidationAttribute
{
    public BCryptPasswordAttribute() : base("La contraseña no puede superar 72 bytes en UTF-8.") { }
    public override bool IsValid(object? value) => value is null ||
        value is string password && Encoding.UTF8.GetByteCount(password) <= 72;
}