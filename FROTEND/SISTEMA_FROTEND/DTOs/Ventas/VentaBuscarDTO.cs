using System;
using System.Collections.Generic;
using System.Text;

namespace SISTEMA_FROTEND.DTOs.Ventas
{
    public class VentaBuscarDTO
    {
        public int id_venta { get; set; }
        public DateTime fecha_venta { get; set; }
        public decimal total { get; set; }

        public List<DetalleVentaBuscarDTO> detalles { get; set; }
            = new();
    }

    public class DetalleVentaBuscarDTO
    {
        public int id_detalle_venta { get; set; }
        public int id_producto { get; set; }
        public string producto { get; set; }
        public int cantidad { get; set; }
        public decimal precio { get; set; }
        public decimal descuento { get; set; }
        public decimal subtotal { get; set; }
    }
}
