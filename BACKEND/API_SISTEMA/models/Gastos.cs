using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.models
{
    public class Gastos
    {
        [Key]
        public int id__gasto { get; set; }
        public int id_sesion_caja { get; set; }
        public int id_usuario { get; set; }
        public string descripcion { get; set; }
        public decimal monto { get; set; }
        public DateTime fecha { get; set; }
        public string observacion { get; set; }

        public SesionCaja sesionCaja { get; set; }
        public Usuario usuario { get; set; }
    }
}
