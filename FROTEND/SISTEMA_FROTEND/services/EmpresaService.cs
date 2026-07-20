using SISTEMA_FROTEND.Conexion;
using SISTEMA_FROTEND.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SISTEMA_FROTEND.services
{
    public class EmpresaService
    {
        private HttpClient Cliente =>
          ApiClient.ObtenerClienteAutenticado();

        public async Task<List<EmpresaDTOs>> ListarEmpresas()
        {
            return await Cliente.GetFromJsonAsync<List<EmpresaDTOs>>
            (
                "Empresa/listar"
            ) ?? new List<EmpresaDTOs>();
        }
    }
}
