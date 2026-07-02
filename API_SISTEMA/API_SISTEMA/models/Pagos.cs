using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.models
{
    public class Pagos
    {
        [Key]
        public int id_pago {  get; set; }
        public int id_venta { get; set; }
        public int id_usuario { get; set; }
        public decimal monto { get; set; }
        public string? metodo_pago { get; set; }
        public string? observacion { get; set; }
        public DateTime fecha_pago { get; set; } =DateTime.Now;

    }
}
