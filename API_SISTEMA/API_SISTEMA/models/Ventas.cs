using API_SISTEMA.DTOs;
using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.models
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

        public int tipo_venta { get; set; }
        public string? origen { get; set; }
        public int id_tipo_cliente { get; set; }
        public int id_estado_venta { get; set; }
        public decimal monto_pagado { get; set; }
        public decimal saldo_pendiente { get; set; }

        public string observacion { get; set; }
        public TipoCliente TipoCliente { get; set; }
        public EstadoVenta? EstadoVenta { get; set; }

        public List<Detalle_venta> DetalleVentas { get; set; } = new();
    }
}
