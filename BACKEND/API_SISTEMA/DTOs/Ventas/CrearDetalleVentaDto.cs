namespace API_SISTEMA.Dtos.Ventas;

public class CrearDetalleVentaDto
    {
        public int? id_producto { get; set; }
        public int? id_surtido { get; set; }
        public List<ComponenteSurtidoVentaDto>? componentes { get; set; }
        public int id_producto_presentacion { get; set; }
        public int cantidad { get; set; }
        public decimal descuento { get; set; }
    }

public sealed class ComponenteSurtidoVentaDto
{
    public int id_producto { get; set; }
    public int cantidad { get; set; }
}
