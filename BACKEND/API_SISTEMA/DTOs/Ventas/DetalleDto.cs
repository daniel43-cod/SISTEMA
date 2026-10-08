using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Dtos.Ventas
{
    public class DetalleDto
    {
        public int id_producto { get; set; }
        public int cantidad { get; set; }
        public decimal descuento { get; set; }
        public int id_producto_presentacion { get; set; }
       



    }
}
