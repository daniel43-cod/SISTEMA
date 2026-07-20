using Microsoft.Extensions.Primitives;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace API_SISTEMA.models
{
    public class RegistroCompras
    {
        [Key]
        public int id_compra { get; set; }
        [Required]
        public int id_empresa   { get; set; }
        [Required]
        public int id_usuario { get; set; } 
        [Required]
        public int id_estado_compra { get; set; }
        [Required]
        public DateTime fecha_ingreso { get; set; }
        public int id_sesion_caja { get; set; }
        [Required]
        public decimal total_compra { get; set; }
        public Usuario usuario { get; set; }
        public Empresa empresa { get; set; }
        public EstadoCompra estado_compra { get; set; }
        public SesionCaja sesioncaja { get; set; }

    }
}
