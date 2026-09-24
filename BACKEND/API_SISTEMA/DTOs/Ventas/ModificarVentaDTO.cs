namespace API_SISTEMA.DTOs.Ventas
{
    public class ModificarVentaDTO
    {
        public int id_cliente { get; set; }

        public string? observacion { get; set; }

        public List<ActualizarDetalleVentaDTO> detalles { get; set; }= new();
    }

    public class ActualizarDetalleVentaDTO
    {
        public int id_producto { get; set; }

        public int id_producto_presentacion { get; set; }

        public int cantidad { get; set; }

        public decimal descuento { get; set; }
    }
}
