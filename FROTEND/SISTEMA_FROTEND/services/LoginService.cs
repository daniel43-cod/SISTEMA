using SISTEMA_FROTEND.Conexion;
using SISTEMA_FROTEND.DTOs.Login;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace SISTEMA_FROTEND.services
{
    public class LoginService
    {
        public async Task<LoginRespuestaDTO?> Login(LoginDTO login)
        {
            HttpClient client = ApiClient.ObtenerClientePublico();

            var response = await client.PostAsJsonAsync("Login/Login", login);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<LoginRespuestaDTO>();
        }

       
    }
}
