using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.DTOs.Compras
{
    public class RegistroComprasDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "El valor debe ser mayor que cero.")]
        public int id_proveedor { get; set; }
        [StringLength(100, ErrorMessage = "La observaci\u00f3n admite hasta 100 caracteres.")]
        public string? observacion { get; set; }
        [Range(typeof(decimal), "0", "99999999.99", ErrorMessage = "El monto est\u00e1 fuera del rango permitido.")]
        public decimal monto_pagado { get; set; }
        [Required(ErrorMessage = "Los detalles son obligatorios.")]
        [MaxLength(100)]
        [MinLength(1, ErrorMessage = "Agrega al menos un producto a la compra.")]
        public List<DetalleCompraDTOs> detalle_compra { get; set; } = new();

    }
}
