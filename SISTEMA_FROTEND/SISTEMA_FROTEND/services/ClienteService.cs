using SISTEMA_FROTEND.DTOs.Cliente;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace SISTEMA_FROTEND.services
{
    public class ClienteService
    {

        private readonly HttpClient _httpClient;

        public ClienteService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:44308/api/");
        }
        public async Task<List<ClienteBuscarDTOs>> BuscarClientes(string texto)
        {
            return await _httpClient.GetFromJsonAsync<List<ClienteBuscarDTOs>>($"Cliente/buscar?texto={Uri.EscapeDataString(texto)}")?? new List<ClienteBuscarDTOs>();
        }

        public async Task<List<ListarClienteDTOs>> ListarClientes()
        {
            return await _httpClient.GetFromJsonAsync<List<ListarClienteDTOs>>("Cliente/listar");
        }
    }
}
