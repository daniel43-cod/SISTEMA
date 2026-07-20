using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.models
{
    public class Inventario_movimiento
    {
        [Key]
       public int id_movimiento {  get; set; }
       public int id_producto { get; set; }
       public int id_usuario { get; set; }
       public string tipo_movimiento {get; set; }
       public string cantidad { get; set; }
       public string motivo { get; set; }
        public DateTime fecha_mocimiento { get; set; }
    }
}
