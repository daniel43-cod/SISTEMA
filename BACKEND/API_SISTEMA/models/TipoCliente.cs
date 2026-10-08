using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Models
{
    public class TipoCliente
    {
        [Key]
        public int id_tipo_cliente { get; set; }
        public string? descripcion { get; set; }
        public bool estado { get; set; }

        //relacion para un producto que tiene muchos registrps
        public ICollection<ProductoPrecio> ProductoPrecios { get; set; }
    }
}
