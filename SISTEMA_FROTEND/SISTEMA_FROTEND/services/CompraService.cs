using SISTEMA_FROTEND.DTOs.Compras;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Text;

namespace SISTEMA_FROTEND.services
{
    public class CompraService
    {

        private readonly HttpClient _httpClient;
        public CompraService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:44308/api/");
        }

        public async Task<ComprasDTOs?> CrearCompra(ComprasDTOs compra)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "crear",
                compra);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content
                    .ReadFromJsonAsync<ComprasDTOs>();
            }

            string error = await response.Content.ReadAsStringAsync();

            throw new Exception(
                $"Código: {(int)response.StatusCode}\n" +
                $"Estado: {response.StatusCode}\n" +
                $"Respuesta:\n{error}");
        }
    }
}
