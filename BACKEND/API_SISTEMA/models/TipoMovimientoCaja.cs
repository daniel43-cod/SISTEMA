using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Models
{
    public class TipoMovimientoCaja
    {
        [Key]
        public int id_tipo_movimiento { get; set; }
        public string nombre_movimiento { get; set; }
        public string naturaleza { get; set; }
    }
}
