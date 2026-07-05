using System;
using System.Collections.Generic;
using System.Text;

namespace SISTEMA_FROTEND.helpers
{
    public class Sesion
    {
        public static int IdUsuario { get; set; }
        public static string Nombre { get; set; } = "";
        public static string Rol { get; set; } = "";
        public static string Token { get; set; } = "";
    }
}
