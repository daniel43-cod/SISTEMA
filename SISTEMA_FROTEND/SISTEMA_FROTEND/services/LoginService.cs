using SISTEMA_FROTEND.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace SISTEMA_FROTEND.services
{
    public class LoginService
    {
        private readonly HttpClient _httpClient;

        public LoginService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:44308/api/");
        }

        public async Task<LoginRespuestaDTO?> Login(LoginDTO login)
        {
            var response = await _httpClient.PostAsJsonAsync("Login/Login", login);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<LoginRespuestaDTO>();
        }
    }
}
