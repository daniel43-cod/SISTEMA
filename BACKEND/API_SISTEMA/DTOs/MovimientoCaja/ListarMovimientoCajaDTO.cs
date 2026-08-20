namespace API_SISTEMA.DTOs.MovimientoCaja
{
    public class ListarMovimientoCajaDTO
    {
        
    public int id_movimiento_caja { get; set; }

        public int id_sesion_caja { get; set; }

        public int id_tipo_movimiento { get; set; }
        public string tipo_movimiento { get; set; }

        public string naturaleza { get; set; }

        public int id_usuario { get; set; }
        public string usuario { get; set; }

        public DateTime fecha_movimiento { get; set; }

        public decimal monto { get; set; }

        public string? descripcion { get; set; }

        public int? id_venta { get; set; }
        public int? id_compra { get; set; }
        public int? id_pago_venta { get; set; }
        public int? id_pago_compra { get; set; }
    }
}
