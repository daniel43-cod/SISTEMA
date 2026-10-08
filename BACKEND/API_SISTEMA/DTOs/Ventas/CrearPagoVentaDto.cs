namespace API_SISTEMA.Dtos.Ventas;

public class CrearPagoVentaDto
    {
        public decimal monto { get; set; }
        public string? metodo_pago { get; set; }
        public string? observacion { get; set; }
    }
