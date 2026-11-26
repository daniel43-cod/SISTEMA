using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Dtos.Compras
{
    public class RegistroComprasDto : IValidatableObject
    {
        public Guid id_operacion { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "El valor debe ser mayor que cero.")]
        public int? id_proveedor { get; set; }
        [StringLength(100)]
        public string? nombre_proveedor { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var tieneNombre = !string.IsNullOrWhiteSpace(nombre_proveedor);
            if (id_proveedor.HasValue == tieneNombre)
                yield return new ValidationResult("Selecciona una distribuidora o ingresa el nombre de una nueva, no ambos.", [nameof(id_proveedor), nameof(nombre_proveedor)]);
            if (tieneNombre && nombre_proveedor!.Any(char.IsControl))
                yield return new ValidationResult("El nombre de la distribuidora contiene caracteres no permitidos.", [nameof(nombre_proveedor)]);
        }
        [StringLength(100, ErrorMessage = "La observaci\u00f3n admite hasta 100 caracteres.")]
        public string? observacion { get; set; }
        [Range(typeof(decimal), "0", "99999999.99", ErrorMessage = "El monto est\u00e1 fuera del rango permitido.")]
        public decimal monto_pagado { get; set; }
        [Required(ErrorMessage = "Los detalles son obligatorios.")]
        [MaxLength(100)]
        [MinLength(1, ErrorMessage = "Agrega al menos un producto a la compra.")]
        public List<DetalleCompraDto> detalle_compra { get; set; } = new();

    }
}
