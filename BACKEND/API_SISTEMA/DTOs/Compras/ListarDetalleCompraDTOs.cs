using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.DTOs.Compras
{
    public class ListarDetalleCompraDTOs
    {
        
        public int id_detalle_compra { get; set; }
        public int id_registro_compra { get; set; }
        public decimal subtotal { get; set; }
        public int id_producto { get; set; }
        public int cantidad { get; set; }
        public string nombre_producto { get; set; }
        public decimal precio { get; set; }
    }
}
