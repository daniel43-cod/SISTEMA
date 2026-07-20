using System;
using System.Collections.Generic;
using System.Text;

namespace SISTEMA_FROTEND.DTOs.Compras
{
    public class ComprasDTOs
    {
        public int id_usuario { get; set; }
        public int id_empresa { get; set; }
        public int id_estado_compra { get; set; }
        public List<DetalleCompraDTOs> detalle_compra { get; set; } = new();
    }
}
