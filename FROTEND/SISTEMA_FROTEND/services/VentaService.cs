using SISTEMA_FROTEND.Conexion;
using SISTEMA_FROTEND.DTOs.Catalogo;
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
        private HttpClient Cliente =>
        ApiClient.ObtenerClienteAutenticado();

        public async Task<List<ListarVentasDTOs>> ListarVentasCajaActiva()
        {
            return await Cliente
                .GetFromJsonAsync<List<ListarVentasDTOs>>(
                    "Venta/listar"
                )
                ?? new List<ListarVentasDTOs>();
        }
        public async Task<CrearVentaDTO?> CrearVenta(CrearVentaDTO   ventaDto)
        {
            var response = await Cliente.PostAsJsonAsync(
                "Venta/crear",
                ventaDto
            );

            if (response.IsSuccessStatusCode)
            {
                return await response.Content
                    .ReadFromJsonAsync<CrearVentaDTO>();
            }

            var error = await response.Content.ReadAsStringAsync();

            throw new Exception(
                $"Código: {(int)response.StatusCode}\n" +
                $"Estado: {response.StatusCode}\n" +
                $"Respuesta:\n{error}"
            );
        }

        //CATALOGO 
        public async Task<List<ProductoCatalogoDTO>> ListarCatalogo()
        {
            var cliente = ApiClient.ObtenerClienteAutenticado();

            var respuesta = await cliente.GetAsync("Venta/catalogo");

            if (respuesta.IsSuccessStatusCode)
            {
                return await respuesta.Content
                    .ReadFromJsonAsync<List<ProductoCatalogoDTO>>()
                    ?? new List<ProductoCatalogoDTO>();
            }

            var error = await respuesta.Content.ReadAsStringAsync();

            throw new Exception(
                $"No se pudo cargar el catálogo.\n" +
                $"Código: {(int)respuesta.StatusCode}\n" +
                $"Estado: {respuesta.StatusCode}\n" +
                $"Respuesta: {error}"
            );
        }

        //buscar venta
        public async Task<List<VentaBuscarDTO>> BuscarVentasClienteCajaActiva(int idCliente)
        {
            HttpClient cliente =
                ApiClient.ObtenerClienteAutenticado();

            var ventas =
                await cliente.GetFromJsonAsync<List<VentaBuscarDTO>>(
                    $"Venta/caja-activa/cliente/{idCliente}"
                );

            return ventas ?? new List<VentaBuscarDTO>();
        }
    }
}
