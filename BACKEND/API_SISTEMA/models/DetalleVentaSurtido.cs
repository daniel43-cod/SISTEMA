using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Models;

[Table("detalle_venta_surtido", Schema = "public")]
[PrimaryKey(nameof(IdDetalleVenta), nameof(IdProducto))]
public class DetalleVentaSurtido
{
    [Column("id_detalle_venta")]
    public int IdDetalleVenta { get; set; }

    [Column("id_producto")]
    public int IdProducto { get; set; }

    // Cantidad total de unidades base de este producto utilizadas en la línea vendida.
    [Range(1, int.MaxValue)]
    [Column("cantidad")]
    public int Cantidad { get; set; }

    // Fotografía del costo promedio por unidad base al registrar la venta.
    [Precision(18, 6)]
    [Column("costo_unitario_venta")]
    public decimal CostoUnitarioVenta { get; set; }

    [ForeignKey(nameof(IdDetalleVenta))]
    public DetalleVenta DetalleVenta { get; set; } = null!;

    [ForeignKey(nameof(IdProducto))]
    public Productos Producto { get; set; } = null!;
}
