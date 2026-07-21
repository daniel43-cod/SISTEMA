using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.models
{
    public class SesionCaja
    {
        [Key]
        public int id_sesion_caja { get; set; }
        public int id_caja { get; set; }
        public int id_usuario_apertura { get; set; } 
        public int? id_usuario_cierre { get; set; }
        public DateTime fecha_apertura { get; set;}
        public decimal monto_inicial { get; set; }
        public decimal monto_esperado { get; set; }
        public decimal monto_contado { get; set; }
        public decimal diferencia { get; set; }
        public string observacion_apertura { get; set; }
        public DateTime? fecha_cierre { get; set;}
        public string? observacion_cierre { get; set; }
        public caja caja { get; set; } = null!;
        public Usuario usuarioapertura { get; set; }= null!;
        public Usuario? usuariocierre { get; set; }
      
    }
}
