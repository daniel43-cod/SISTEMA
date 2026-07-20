using SISTEMA_FROTEND.Conexion;
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
        private HttpClient Cliente =>
        ApiClient.ObtenerClienteAutenticado();

        public async Task<List<DetalleDTOs>> ListarDetalle(int idVenta)
        {
            return await Cliente.GetFromJsonAsync<List<DetalleDTOs>>
            (
                $"DetalleVenta_/listar/{idVenta}"
            ) ?? new List<DetalleDTOs>();
        }
    }

    
}
