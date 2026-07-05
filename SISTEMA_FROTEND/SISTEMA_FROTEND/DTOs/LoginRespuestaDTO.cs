using System;
using System.Collections.Generic;
using System.Text;

namespace SISTEMA_FROTEND.DTOs
{
    public class LoginRespuestaDTO
    {
        public int id_usuario { get; set; }
        public string nombre { get; set; } = "";
        public string rol { get; set; } = "";
        public string token { get; set; } = "";
    }
}
