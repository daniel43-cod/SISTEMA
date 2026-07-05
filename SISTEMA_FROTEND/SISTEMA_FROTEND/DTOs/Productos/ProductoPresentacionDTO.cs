using System;
using System.Collections.Generic;
using System.Text;

namespace SISTEMA_FROTEND.DTOs.Productos
{
    public class ProductoPresentacionDTO
    {
        public string descripcion { get; set; }
        public int unidades_equivalentes { get; set; }
        public decimal precio { get; set; }
    }
}
