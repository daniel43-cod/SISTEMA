using SISTEMA_FROTEND.DTOs.Ventas;
using SISTEMA_FROTEND.models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace SISTEMA_FROTEND.services
{
    internal class VentaService
    {


        private readonly HttpClient _httpClient;
        public VentaService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:44308/api/");
        }

        public async Task<List<ListarVentasDTOs>> ListarVentas()
        {
            return await _httpClient.GetFromJsonAsync<List<ListarVentasDTOs>>("Venta/listar");
        }

        public async Task<VentaDTOs?> CrearVenta(VentaDTOs ventaDto)
        {
            var response = await _httpClient.PostAsJsonAsync("Venta/crear", ventaDto);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<VentaDTOs>();
            }

            var error = await response.Content.ReadAsStringAsync();

            throw new Exception(
                $"Código: {(int)response.StatusCode}\n" +
                $"Estado: {response.StatusCode}\n" +
                $"Respuesta:\n{error}");
        }

    }
}
