namespace API_SISTEMA.Dtos.Ventas
{
    public class ListarDetalleDto
    {
        public int? id_producto { get; set; }
        public int? id_surtido { get; set; }
        public List<ComponenteDetalleVentaDto> componentes { get; set; } = [];
        public required string nombre_producto { get; set; }
        public int cantidad { get; set; }
        public decimal descuento { get; set; }
        public int id_producto_presentacion { get; set; }
        public string? descripcion_resentacion { get; set; }
        public decimal precio { get; set; }
    }
    public sealed record ComponenteDetalleVentaDto(int id_producto, string nombre, int cantidad);
}
