using SISTEMA_FROTEND.Conexion;
using SISTEMA_FROTEND.DTOs.Caja;
using SISTEMA_FROTEND.DTOs.Compras;
using SISTEMA_FROTEND.DTOs.Productos;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace SISTEMA_FROTEND.services
{
    public class CajaService
    {
        private readonly HttpClient Cliente;

        public CajaService()
        {
            Cliente = ApiClient.ObtenerClienteAutenticado();
        }

        //Apertura Caja
        public async Task<AperturaCajaDT0s?> AbrirCaja(AperturaCajaDT0s apertura)
        {
            var response = await Cliente.PostAsJsonAsync(
                "Caja/Abrir",
                apertura
            );

            if (response.IsSuccessStatusCode)
            {
                return await response.Content
                    .ReadFromJsonAsync<AperturaCajaDT0s>();
            }

            string error = await response.Content.ReadAsStringAsync();

            throw new Exception(
                $"Código: {(int)response.StatusCode}\n" +
                $"Estado: {response.StatusCode}\n" +
                $"Respuesta:\n{error}"
            );
        }

    }
}
