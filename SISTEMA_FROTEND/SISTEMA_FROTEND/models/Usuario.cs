using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SISTEMA_FROTEND.models
{
    internal class Usuario
    {

        [Key]
        public int id_usuario { get; set; }
        public int id_rol { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string usuario { get; set; }
        public string correo { get; set; }
        public string telefono { get; set; }
        public string password { get; set; }
        public bool estado { get; set; }
        public DateTime fecha_Creacion { get; set; }
    }
}
