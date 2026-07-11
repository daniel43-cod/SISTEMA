using SISTEMA_FROTEND.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace SISTEMA_FROTEND.services
{
    public class EmpresaService
    {
       private readonly HttpClient _httpClient;
        public EmpresaService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:44308/api/");
        }

        public async Task<List<EmpresaDTOs>> ListarEmpresas()
        {
            return await _httpClient.GetFromJsonAsync<List<EmpresaDTOs>>("Empresa/listar");
        }
    }
}
