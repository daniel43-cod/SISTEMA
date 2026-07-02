using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.DTOs
{
    public class DetalleDTOs
    {
        public int id_producto { get; set; }
        //public string tipo_cliente { get; set; }
        public int cantidad { get; set; }
        public decimal? descuento { get; set; }
      

        

    }
}
