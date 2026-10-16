namespace API_SISTEMA.Dtos.Gastos
{
    using System.ComponentModel.DataAnnotations;

    public class IngresarGastoDto : IValidatableObject
    {
     
      
        [Required(ErrorMessage = "La descripción del gasto es obligatoria.")]
        [StringLength(300, ErrorMessage = "La descripción admite hasta 300 caracteres.")]
        public string descripcion { get; set; } = string.Empty;
        [Range(typeof(decimal), "0.01", "99999999.99",ErrorMessage = "El monto debe estar entre 0.01 y 99999999.99.")]
        [Required(ErrorMessage ="El monto del gasto es obligatorio")]
        public decimal monto { get; set; }
        [StringLength(500, ErrorMessage = "La observación admite hasta 500 caracteres.")]
        public string? observacion { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (decimal.Round(monto, 2) != monto)
                yield return new ValidationResult("El monto admite hasta dos decimales.", new[] { nameof(monto) });
        }
    }
}
