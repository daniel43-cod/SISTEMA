using System;
using System.Collections.Generic;
using System.Text;

namespace SISTEMA_FROTEND.DTOs.Ventas
{
    public class DetalleDTOs
    {
        public int id_producto { get; set; }
        public int cantidad { get; set; }
        public decimal descuento { get; set; }
        public int id_producto_presentacion { get; set; }
    }
}
