using System;
using System.Collections.Generic;
using System.Drawing.Imaging.Effects;
using System.Text;

namespace SISTEMA_FROTEND.DTOs.Productos
{
    public class ProductoDTOs
    {
        public string codigo_barra { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int id_categoria { get; set; }
        public decimal precio_compra { get; set; }
        public int stock { get; set; }
        public int stock_minimo { get; set; }
        public decimal impuesto { get; set; }
        public string? imagen { get; set; }
        public decimal costo_unitario { get; set; }
        public List<ProductoPresentacionDTO> presentaciones { get; set; }
        public List<ProductoPrecioDTOs> precios { get; set; }

    }
}
