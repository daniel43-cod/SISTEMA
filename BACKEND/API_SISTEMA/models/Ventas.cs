using API_SISTEMA.Dtos;
using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Models
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
        public required string origen { get; set; }
        public int id_estado_venta { get; set; }
        public int id_sesion_caja { get; set; }
        public decimal monto_pagado { get; set; }
        public decimal saldo_pendiente { get; set; }
        public string? observacion { get; set; }
        public EstadoVenta EstadoVenta { get; set; } = null!;
        public Usuario usuario { get; set; } =null!;
        public Cliente cliente { get; set; }=null!;
        public SesionCaja sesionCaja { get; set; }=null!;
        public List<DetalleVenta> DetalleVentas { get; set; } = new();
    }
}
