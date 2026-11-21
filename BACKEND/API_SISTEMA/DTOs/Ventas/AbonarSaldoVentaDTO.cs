namespace API_SISTEMA.Dtos.Ventas
{
    public class AbonarSaldoVentaDto
    {
        public Guid id_operacion { get; set; }
        public int id_venta { get; set; }
        public decimal monto { get; set; }
        public string? observacion { get; set; }
    }
}
