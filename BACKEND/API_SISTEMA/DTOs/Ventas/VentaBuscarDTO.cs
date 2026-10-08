namespace API_SISTEMA.Dtos.Ventas
{
    public class VentaBuscarDto
    {
        public int id_venta { get; set; }
        public DateTime fecha_venta { get; set; }
        public decimal total { get; set; }

        public List<DetalleVentaBuscarDto> detalles { get; set; }
            = new();
    }

    
}
