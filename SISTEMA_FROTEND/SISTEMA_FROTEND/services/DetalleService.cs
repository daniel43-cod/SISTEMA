using SISTEMA_FROTEND.DTOs.Ventas;
using SISTEMA_FROTEND.models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace SISTEMA_FROTEND.services
{
    public class DetalleService
    {

        private readonly HttpClient _httpClient;
        public DetalleService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:44308/api/");
        }


        public async Task<List<DetalleDTOs>> ListarDetalle(int _idventa)
        {
            return await _httpClient.GetFromJsonAsync<List<DetalleDTOs>>($"DetalleVenta_/Detalle/{_idventa}");
        }
    }

    
}
