using System;
using System.Collections.Generic;
using System.Text;

namespace SISTEMA_FROTEND.DTOs.Cliente
{
    public class ClienteBuscarDTOs
    {
        public int id_Cliente { get; set; }
        public string nombre { get; set; } = string.Empty;
        public string apellido { get; set; }
        public string nit { get; set; }
        public string dpi { get; set; }
        public string telefono { get; set; }
        public string correo_electronico { get; set; }
        public string direccion { get; set; }
        public string nombreCompleto => $"{nombre} {apellido}";
    }
}
