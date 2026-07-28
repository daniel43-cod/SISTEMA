using SISTEMA_FROTEND.Utilidades;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace SISTEMA_FROTEND.Conexion
{
    public class ApiClient
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:44308/api/")
        };

        public static HttpClient ObtenerClientePublico()
        {
            // Elimina cualquier token anterior.
            // Se usa principalmente para iniciar sesión.
            _httpClient.DefaultRequestHeaders.Authorization = null;

            return _httpClient;
        }

        public static HttpClient ObtenerClienteAutenticado()
        {
            if (!ConfiguracionApp.EstaConfigurada())
            {
                throw new Exception(
                    "El servidor y la caja no están configurados."
                );
            }

            if (string.IsNullOrWhiteSpace(SesionUsuario.Token))
            {
                throw new UnauthorizedAccessException(
                    "No existe una sesión iniciada."
                );
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    SesionUsuario.Token
                );

            return _httpClient;
        }
    }
}
