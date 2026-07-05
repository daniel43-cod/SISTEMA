using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SISTEMA_FROTEND.models
{
    public class Ventas
    {
        [Key]
       
        public int id_ventas { get; set; }
        public string? nombre_cliente { get; set; }
        public decimal subtotal { get; set; }
        public decimal? descuento { get; set; }
        public decimal? impuesto { get; set; }
        public decimal total { get; set; }
        public DateTime fecha_venta { get; set; } = DateTime.Now;
        public int? id_cliente { get; set; }
        public int? id_usuario { get; set; }
        public string tipo_venta { get; set; }
        public string origen { get; set; }
        public string estado_venta { get; set; }
        public decimal saldo_pendiente { get; set; }
    }
}
