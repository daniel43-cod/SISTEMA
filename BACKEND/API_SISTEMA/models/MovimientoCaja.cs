using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.models
{
    public class MovimientoCaja
    {
        [Key]
        public int id_movimiento_caja { get; set; }
        public int id_sesion_caja { get; set; }
        public int id_tipo_movimiento { get; set; }
        public int id_usuario { get; set; }
        public DateTime fecha_movimiento { get; set; }
        public decimal monto { get; set; }
        public string? descripcion { get; set; }
        public int? id_venta { get; set; }
        public int? id_compra { get; set; }
        public int? id_pago_venta { get; set; }
        public int? id_pago_compra { get; set; }

        public SesionCaja sesionCaja { get; set; }
        public TipoMovimientoCaja tipoMovimientoCaja { get; set; }
        public Usuario usuario { get; set; }
        public Ventas venta { get; set; }
        public RegistroCompras RegistroCompras { get; set; }
        public Pagos pagos { get; set; }
        public PagosCompra pagosCompra { get; set; }
    }
}
