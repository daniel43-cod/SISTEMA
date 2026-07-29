using System;
using System.Collections.Generic;
using System.Text;

namespace SISTEMA_FROTEND.DTOs.Catalogo
{
    public class PresentacionCatalogoDTO
    {
        public int id_producto_presentacion { get; set; }

        public string presentacion { get; set; } = string.Empty;

        public int unidades_equivalentes { get; set; }

        public decimal precio { get; set; }
    }
}
