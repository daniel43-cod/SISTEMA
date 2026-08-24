using SISTEMA_FROTEND.forms;
using SISTEMA_FROTEND.presentacion;
using SISTEMA_FROTEND.Utilidades;

namespace SISTEMA_FROTEND
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            // SOLO PARA PRUEBAS
            ConfiguracionApp.Guardar(
                1,
                "CAJA 1",
                "https://localhost:44308/api/"
            );
            Application.Run(new LOGIN());
        }
    }
}