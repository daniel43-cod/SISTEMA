using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Dtos.Productos;

public sealed class BusquedaProductoAdminDto : IValidatableObject
{
    [StringLength(200)] public string? Nombre { get; set; }
    [StringLength(100)] public string? CodigoBarra { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var nombre = Nombre?.Trim();
        var codigo = CodigoBarra?.Trim();
        if (string.IsNullOrEmpty(nombre) == string.IsNullOrEmpty(codigo))
            yield return new ValidationResult("Envía nombre o codigoBarra, uno a la vez.");
        if (!string.IsNullOrEmpty(nombre) && (nombre.Length < 3 || nombre.Any(char.IsControl)))
            yield return new ValidationResult("El nombre debe tener al menos tres caracteres y no admitir controles.");
        if (!string.IsNullOrEmpty(codigo) && (CodigoBarra!.Any(char.IsControl) || codigo.Any(char.IsWhiteSpace)))
            yield return new ValidationResult("El código no admite espacios internos ni controles.");
    }
}


