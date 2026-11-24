using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Models;

[Table("surtido_producto", Schema = "public")]
[PrimaryKey(nameof(IdSurtido), nameof(IdProducto))]
public class SurtidoProducto
{
    [Column("id_surtido")]
    public int IdSurtido { get; set; }

    [Column("id_producto")]
    public int IdProducto { get; set; }

    [ForeignKey(nameof(IdSurtido))]
    public Surtido Surtido { get; set; } = null!;

    [ForeignKey(nameof(IdProducto))]
    public Productos Producto { get; set; } = null!;
}
