using SISTEMA_FROTEND.Conexion;
using SISTEMA_FROTEND.DTOs.Cliente;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace SISTEMA_FROTEND.services
{
    public class ClienteService
    {
        public async Task<List<ClienteBuscarDTOs>> BuscarClientes(string texto)
        {
            HttpClient cliente = ApiClient.ObtenerClienteAutenticado();

            return await cliente.GetFromJsonAsync<List<ClienteBuscarDTOs>>
            (
                $"Cliente/buscar?texto={Uri.EscapeDataString(texto)}"
            ) ?? new List<ClienteBuscarDTOs>();
        }

        public async Task<List<ListarClienteDTOs>> ListarClientes()
        {
            HttpClient cliente = ApiClient.ObtenerClienteAutenticado();

            return await cliente.GetFromJsonAsync<List<ListarClienteDTOs>>
            (
                "Cliente/listar"
            ) ?? new List<ListarClienteDTOs>();
        }
    }
}
