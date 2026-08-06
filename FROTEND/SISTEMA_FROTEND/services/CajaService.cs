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
            var response = await Cliente.PostAsJsonAsync("Caja/Abrir",apertura);

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

        public async Task<List<ListarSesionesCajaDTO>> ListarSesiones()
        {
            var response = await Cliente.GetAsync("Caja/ListarSesionesCaja");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content
                    .ReadFromJsonAsync<List<ListarSesionesCajaDTO>>()
                    ?? new List<ListarSesionesCajaDTO>();
            }

            throw new Exception(
                $"Código: {(int)response.StatusCode}\n" +
                $"Estado: {response.StatusCode}\n" +
                $"Respuesta:\n{await response.Content.ReadAsStringAsync()}"
            );
        }

        public async Task<RespuetaCierreDTO> CerrarCaja(CierreCajaDTOs cierreCaja)
        {
            var cliente = ApiClient.ObtenerClienteAutenticado();

            var response = await cliente.PostAsJsonAsync(
                "Caja/cerrar",
                cierreCaja
            );

            if (response.IsSuccessStatusCode)
            {
                var resultado = await response.Content
                    .ReadFromJsonAsync<RespuetaCierreDTO>();

                return resultado
                    ?? throw new Exception(
                        "La API cerró la caja, pero no devolvió los datos del cierre."
                    );
            }

            string error = await response.Content.ReadAsStringAsync();

            throw new Exception(
                $"No se pudo cerrar la caja.\n" +
                $"Código: {(int)response.StatusCode}\n" +
                $"Respuesta: {error}"
            );
        }

    }
}
