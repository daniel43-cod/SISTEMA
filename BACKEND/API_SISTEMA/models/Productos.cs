using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.models
{
    public class Productos
    {
        [Key]
        public int id_producto { get; set; }
        public string? codigo_barra { get; set; }
        public string nombre { get; set; } =string.Empty;
        // Cada producto pertenece a una marca; esta determina su categoría.
        [System.ComponentModel.DataAnnotations.Schema.Column("id_marca")]
        public int IdMarca { get; set; }
        public Marca Marca { get; set; } = null!;
        public decimal? precio_compra { get; set; }
        public int? stock { get; set; }
        public int? stock_minimo { get; set; }
        [MaxLength(2048)]
        public string? imagen { get; set; }
        public decimal? costo_unitario { get; set; }
        public decimal? impuesto { get; set; } 
        public DateTime fecha_creacion {  get; set; }
        public List<Detalle_venta> DetalleVentas { get; set; } = new();
        public ICollection<Producto_precio> ProductoPrecios { get; set; } = new List<Producto_precio>();

        public ICollection<Producto_Presentacion> ProductoPresentaciones { get; set; } = new List<Producto_Presentacion>();

    }
}
