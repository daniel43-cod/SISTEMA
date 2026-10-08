namespace API_SISTEMA.Dtos.Ventas
{
    public class ListarDetalleDto
    {
        public int id_producto { get; set; }
        public string nombre_producto { get; set; }
        public int cantidad { get; set; }
        public decimal descuento { get; set; }
        public int id_producto_presentacion { get; set; }
        public string descripcion_resentacion { get; set; }
        public decimal precio { get; set; }
    }
}
