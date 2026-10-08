using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_SISTEMA.Models
{
    public class Presentacion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_presentacion")]
        public int IdPresentacion { get; set; }

        [MaxLength(100)]
        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("estado")]
        public bool? Estado { get; set; }
    }
}
