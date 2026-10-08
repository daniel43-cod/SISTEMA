namespace API_SISTEMA.Dtos.Ventas
{
    public class CrearVentaDto
    {
        public int id_cliente { get; set; }

        public CrearClienteVentaDto? clienteNuevo { get; set; }

        public string? origen { get; set; }

        public string? observacion { get; set; }

        public List<CrearDetalleVentaDto> detalles { get; set; } = new();

        public CrearPagoVentaDto? pago { get; set; }
    }

    

    

    

}
