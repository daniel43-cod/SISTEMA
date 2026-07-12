using SISTEMA_FROTEND.DTOs.Compras;
using SISTEMA_FROTEND.DTOs.Ventas;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;

namespace SISTEMA_FROTEND.services
{
    public class CompraService
    {

        private readonly HttpClient _httpClient;
        public CompraService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:44308/api/Compra/");
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

        //listar las compras
        public async Task<List<ListarComprasDTOs>> ListarCompras()
        {
            return await _httpClient.GetFromJsonAsync<List<ListarComprasDTOs>>("listar")?? new List<ListarComprasDTOs>();
        }

        public async Task<List<ListarDetalleComprasDTOs>> ListarDetalleCompras(int id_compra)
        {
            return await _httpClient.GetFromJsonAsync<List<ListarDetalleComprasDTOs>>($"detalle/{id_compra}") ?? new List<ListarDetalleComprasDTOs>();
        }


    }
}
