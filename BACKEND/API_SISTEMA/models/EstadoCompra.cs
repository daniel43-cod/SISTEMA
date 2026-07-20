using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.models
{
    public class EstadoCompra
    {
        [Key]
        public int id_estado_compra { get; set; }
        [Required]
        public string nombre_estado_compra { get; set; }
        public string descripcion { get; set; }
    }
}
