using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_SISTEMA.Models;

[Table("surtido", Schema = "public")]
public class Surtido
{
    [Key]
    [Column("id_surtido")]
    public int IdSurtido { get; set; }

    [Required, MaxLength(200)]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("estado")]
    public bool Estado { get; set; } = true;

    [Column("id_marca")]
    public int IdMarca { get; set; }
    public Marca Marca { get; set; } = null!;

    // Productos que el administrador permite combinar; no representa existencias propias.
    public ICollection<SurtidoProducto> Productos { get; set; } = new List<SurtidoProducto>();
}
