using System.ComponentModel.DataAnnotations;
namespace API_SISTEMA.Dtos.Compras
{
    public class PagosCompraDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "El valor debe ser mayor que cero.")]
        public int id_compra { get; set; }
        [Range(typeof(decimal), "0.01", "99999999.99", ErrorMessage = "El monto est\u00e1 fuera del rango permitido.")]
        public decimal monto { get; set; }
    }
}
