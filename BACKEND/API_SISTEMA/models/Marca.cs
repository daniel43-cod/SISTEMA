using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_SISTEMA.Models
{
    public class Marca
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_marca")]
        public int IdMarca { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(2048)]
        [Column("ruta_imagen")]
        public string? UrlImagen { get; set; }

        [Column("estado")]
        public bool Estado { get; set; }

        // Cada marca pertenece a una categoría obligatoria.
        [Column("id_categoria")]
        public int IdCategoria { get; set; }

        // Navegación para consultar la categoría asociada mediante Entity Framework.
        public Categoria Categoria { get; set; } = null!;
    }
}
