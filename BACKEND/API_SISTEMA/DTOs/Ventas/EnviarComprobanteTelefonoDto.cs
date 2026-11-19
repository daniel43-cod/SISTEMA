using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Dtos.Ventas;

public sealed class EnviarComprobanteTelefonoDto
{
    [Required, StringLength(30)] public string Telefono { get; set; } = "";
}
