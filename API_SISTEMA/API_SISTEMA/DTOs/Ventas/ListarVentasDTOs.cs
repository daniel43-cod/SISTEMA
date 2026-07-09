namespace API_SISTEMA.DTOs.Ventas
{
    public class ListarVentasDTOs
    {
        public int id_ventas { get; set; }
        public decimal subtotal { get; set; }
        public decimal? descuento { get; set; }
        public decimal impuesto { get; set; }
        public decimal total { get; set; }
        public DateTime fecha_venta { get; set; }

        public int id_cliente { get; set; }
        public string cliente { get; set; }

        public int id_usuario { get; set; }
        public string usuario { get; set; }

        public string estado { get; set; }
        public string origen { get; set; }

        public decimal monto_pagado { get; set; }
        public decimal saldo_pendiente { get; set; }
        public string? observacion { get; set; }
    }
}
