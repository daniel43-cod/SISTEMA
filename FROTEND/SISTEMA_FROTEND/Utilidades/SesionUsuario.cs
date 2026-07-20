using System;
using System.Collections.Generic;
using System.Text;

namespace SISTEMA_FROTEND.Utilidades
{
    public static class SesionUsuario
    {//guarda los datos del usuario en memoria una ves que inicia sesion 
        public static int IdUsuario { get; private set; }

        public static string Nombre { get; private set; } = string.Empty;

        public static string Rol { get; private set; } = string.Empty;

        public static string Token { get; private set; } = string.Empty;

        public static bool EstaAutenticado =>
            !string.IsNullOrWhiteSpace(Token);

        public static void IniciarSesion(
            int idUsuario,
            string nombre,
            string rol,
            string token)
        {
            IdUsuario = idUsuario;
            Nombre = nombre;
            Rol = rol;
            Token = token;
        }

        public static void CerrarSesion()
        {
            IdUsuario = 0;
            Nombre = string.Empty;
            Rol = string.Empty;
            Token = string.Empty;
        }
    }
}

