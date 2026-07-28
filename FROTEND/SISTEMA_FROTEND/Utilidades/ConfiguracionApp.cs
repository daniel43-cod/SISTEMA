using System;
using System.Collections.Generic;
using System.Text;

namespace SISTEMA_FROTEND.Utilidades
{
    public class ConfiguracionApp
    {
        public static int IdCaja =>
       Properties.Settings.Default.IdCaja;

        public static string NombreCaja =>
            Properties.Settings.Default.NombreCaja;

        public static string ApiUrl =>
            Properties.Settings.Default.ApiUrl;

        public static bool EstaConfigurada()
        {
            return IdCaja > 0 &&
                   !string.IsNullOrWhiteSpace(ApiUrl);
        }

        public static void Guardar(
            int idCaja,
            string nombreCaja,
            string apiUrl)
        {
            Properties.Settings.Default.IdCaja = idCaja;
            Properties.Settings.Default.NombreCaja = nombreCaja;
            Properties.Settings.Default.ApiUrl = apiUrl.TrimEnd('/') + "/";

            Properties.Settings.Default.Save();
        }

    }
}
