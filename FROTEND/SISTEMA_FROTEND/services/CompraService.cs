using SISTEMA_FROTEND.Conexion;
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
        private HttpClient Cliente =>
            ApiClient.ObtenerClienteAutenticado();

        public async Task<RegistroComprasDTO?> CrearCompra(RegistroComprasDTO compra)
        {
            var response = await Cliente.PostAsJsonAsync(
                "Compra/crear",
                compra
            );

            if (response.IsSuccessStatusCode)
            {
                return await response.Content
                    .ReadFromJsonAsync<RegistroComprasDTO>();
            }

            string error = await response.Content.ReadAsStringAsync();

            throw new Exception(
                $"Código: {(int)response.StatusCode}\n" +
                $"Estado: {response.StatusCode}\n" +
                $"Respuesta:\n{error}"
            );
        }

        public async Task<List<ListarComprasDTOs>> ListarCompras()
        {
            return await Cliente
                .GetFromJsonAsync<List<ListarComprasDTOs>>(
                    "Compra/listar"
                )
                ?? new List<ListarComprasDTOs>();
        }

        public async Task<List<ListarDetalleComprasDTOs>> ListarDetalleCompras(
            int id_compra)
        {
            return await Cliente
                .GetFromJsonAsync<List<ListarDetalleComprasDTOs>>(
                    $"Compra/detalle/{id_compra}"
                )
                ?? new List<ListarDetalleComprasDTOs>();
        }
    }
}
