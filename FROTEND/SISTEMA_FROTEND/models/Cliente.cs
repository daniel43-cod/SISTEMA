using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SISTEMA_FROTEND.models
{
    public class Cliente
    {
        [Key]

        public int id_Cliente { get; set; }
        public string nombre { get; set; } = string.Empty;
        public string apellido { get; set; }
        public int nit { get; set; }
        public string dpi { get; set; }
        public string telefono { get; set; }
        public string correo_electronico { get; set; }
        public string direccion { get; set; }
        public DateTime fecha_Creacion { get; set; }
        public string tipo_cliente { get; set; }
        public decimal limite_Credito { get; set; }
        public bool estado { get; set; }
    }
}
