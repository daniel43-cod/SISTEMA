using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SISTEMA_FROTEND.models
{
    public class categoria
    {
        [Key]
        public int id_categoria { get; set; }
        public string nombre_categoria { get; set; }
        public string descripcion { get; set; }
        public bool estado { get; set; }
        public DateTime fecha_Creacion { get; set; }
    }
}
