using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_SISTEMA.Dtos.Productos;

public sealed class ActualizarProductoConImagenDto : ActualizarProductoDto
{
    public IFormFile? Imagen { get; set; }
}
