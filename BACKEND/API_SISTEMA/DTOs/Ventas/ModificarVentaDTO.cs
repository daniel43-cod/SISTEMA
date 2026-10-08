namespace API_SISTEMA.Dtos.Ventas
{
    public class ModificarVentaDto
    {
        public int id_cliente { get; set; }

        public string? observacion { get; set; }

        public List<ActualizarDetalleVentaDto> detalles { get; set; }= new();
    }

    
}
