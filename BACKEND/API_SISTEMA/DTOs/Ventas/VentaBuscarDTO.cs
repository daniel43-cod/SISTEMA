namespace API_SISTEMA.Dtos.Ventas
{
    public class VentaBuscarDto
    {
        public int id_venta { get; set; }
        public string? tipo_identificacion_receptor { get; set; }
        public string? identificacion_receptor { get; set; }
        public string? nombre_receptor { get; set; }
        public DateTime fecha_venta { get; set; }
        public decimal total { get; set; }

        public List<DetalleVentaBuscarDto> detalles { get; set; }
            = new();
    }

    
}
