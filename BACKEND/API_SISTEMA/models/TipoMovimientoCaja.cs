using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.models
{
    public class TipoMovimientoCaja
    {
        [Key]
        public int tipo_movimiento_caja { get; set; }
        public string nombre_movimiento { get; set; }
        public string naturaleza { get; set; }
    }
}
