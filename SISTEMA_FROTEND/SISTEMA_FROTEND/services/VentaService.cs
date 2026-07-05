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

        public async Task<List<Ventas>> ListarVentas()
        {
            return await _httpClient.GetFromJsonAsync<List<Ventas>>("Venta");
        }

        public async Task<VentaDTOs?> CrearVenta(DetalleDTOs venta)
        {
            var response = await _httpClient.PostAsJsonAsync("Ventas/crear", venta);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<VentaDTOs>();
            }

            string error = await response.Content.ReadAsStringAsync();
            throw new Exception(error);
        }


    }
}
