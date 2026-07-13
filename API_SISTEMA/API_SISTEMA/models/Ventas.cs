using API_SISTEMA.DTOs;
using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.models
{
    public class Ventas
    {
        [Key]
        public int id_ventas { get; set; }
        public decimal subtotal { get; set; }
        public decimal? descuento { get; set; }
        public decimal impuesto { get; set; }
        public decimal total { get; set; }
        public DateTime fecha_venta { get; set; } = DateTime.Now;
        public int id_cliente { get; set; }
        public int id_usuario { get; set; }
        public decimal ganancia_total { get; set; }
        public string origen { get; set; }
        public int id_estado_venta { get; set; }
        public int id_sesion_caja { get; set; }
        public decimal monto_pagado { get; set; }
        public decimal saldo_pendiente { get; set; }
        public string? observacion { get; set; }
        public EstadoVenta EstadoVenta { get; set; }
        public Usuario usuario { get; set; }
        public Cliente cliente { get; set; }
        public SesionCaja sesionCaja { get; set; }
        public List<Detalle_venta> DetalleVentas { get; set; } = new();
    }
}
