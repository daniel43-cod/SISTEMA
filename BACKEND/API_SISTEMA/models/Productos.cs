using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API_SISTEMA.Models
{
    public class Productos
    {
        [Key]
        [Column("id_producto")]
        [JsonPropertyName("id_producto")]
        public int IdProducto { get; set; }
        [Column("codigo_barra")]
        [JsonPropertyName("codigo_barra")]
        public string? CodigoBarra { get; set; }
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;
        // Cada producto pertenece a una marca; esta determina su categoría.
        [Column("id_marca")]
        public int IdMarca { get; set; }
        public Marca Marca { get; set; } = null!;
        // Costo promedio ponderado por unidad base; se usa para calcular la ganancia.
        [Column("precio_promedio_compra")]
        public decimal? PrecioPromedioCompra { get; set; } = 0m;
        [Column("stock")]
        public int? Stock { get; set; }
        [Column("stock_minimo")]
        [JsonPropertyName("stock_minimo")]
        public int? StockMinimo { get; set; }   
        [MaxLength(2048)]
        [Column("imagen")]
        public string? Imagen { get; set; }
        // Campo anterior: se conserva por compatibilidad con la base de datos.
        [Column("costo_unitario")]
        [JsonPropertyName("costo_unitario")]
        public decimal? CostoUnitario { get; set; }
        [Column("impuesto")]
        public decimal? Impuesto { get; set; }
        [Column("fecha_creacion")]
        [JsonPropertyName("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }
        public List<DetalleVenta> DetalleVentas { get; set; } = new();
        public ICollection<ProductoPrecio> ProductoPrecios { get; set; } = new List<ProductoPrecio>();

        public ICollection<ProductoPresentacion> ProductoPresentaciones { get; set; } = new List<ProductoPresentacion>();

    }
}
