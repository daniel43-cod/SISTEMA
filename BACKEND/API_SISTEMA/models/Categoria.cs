using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace API_SISTEMA.Models
{
    public class Categoria
    {
        [Key]
        [Column("id_categoria")]
        public int IdCategoria { get; set; }
        [Required]
        [Column("nombre_categoria")]
        public  string nombreCategoria { get; set; } = string.Empty;
        [Column("estado")]
        public bool Estado {  get; set; }
        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }
        [Column("url_imagen")]
        public string? UrlImagen { get; set; }

    }
}
