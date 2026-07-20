using System;
using System.Collections.Generic;
using System.Text;

namespace SISTEMA_FROTEND.DTOs.Cliente
{
    public class ClienteNuevoDTOs
    {
        public string nombre { get; set; }
        public string nit { get; set; }
        public string dpi { get; set; }
        public string telefono { get; set; }
        public string correo_electronico { get; set; }
        public string direccion { get; set; }
        public decimal limite_Credito { get; set; }
    }
}
