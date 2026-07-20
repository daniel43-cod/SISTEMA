using System;
using System.Collections.Generic;
using System.Text;

namespace SISTEMA_FROTEND.DTOs.Compras
{
    public class DetalleCompraDTOs
    {
        public int id_producto { get; set; }
        public int cantidad { get; set; }
        public decimal precio { get; set; }
    }
}
