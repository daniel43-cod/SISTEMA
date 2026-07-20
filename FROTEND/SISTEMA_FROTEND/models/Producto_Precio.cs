using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SISTEMA_FROTEND.models
{
    public class Producto_Precio
    {
        [Key]
        public int id_producto_precio { get; set; }
        public int id_producto { get; set; }
        public string tipo_cliente { get; set; }
        public decimal precio { get; set; }
        public bool estado { get; set; }
    }
}
