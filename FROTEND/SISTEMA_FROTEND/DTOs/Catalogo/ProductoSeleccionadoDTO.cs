using System;
using System.Collections.Generic;
using System.Text;

namespace SISTEMA_FROTEND.DTOs.Catalogo
{
    public class ProductoSeleccionadoDTO
    {
        public int id_producto { get; set; }
        public int id_producto_presentacion { get; set; }
        public string nombre_producto { get; set; } = string.Empty;
        public string presentacion { get; set; } = string.Empty;
        public int cantidad { get; set; }
        public decimal precio { get; set; }
        public int stock { get; set; }
        public int unidades_equivalentes { get; set; }
    }
}
