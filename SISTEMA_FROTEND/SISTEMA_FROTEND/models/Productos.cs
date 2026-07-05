using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SISTEMA_FROTEND.models
{
        public class Productos
        {
            [Key]
            public int id_producto { get; set; }
            public string codigo_barra { get; set; }
            public string nombre { get; set; }
            public string descripcion { get; set; }
            public int id_categoria { get; set; }
            public decimal precio_compra { get; set; }
            public int stock { get; set; }
            public int stock_minimo { get; set; }
            public string? imagen { get; set; }
            public decimal impuesto { get; set; }
            public DateTime fecha_creacion { get; set; }
        }
    
}
