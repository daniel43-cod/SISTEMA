using SISTEMA_FROTEND.Conexion;
using SISTEMA_FROTEND.DTOs.Cliente;
using SISTEMA_FROTEND.DTOs.Compras;
using SISTEMA_FROTEND.DTOs.Productos;
using SISTEMA_FROTEND.models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace SISTEMA_FROTEND.services
{
    public class ProductoService
    {
        private readonly HttpClient _httpClient;
        public ProductoService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:44308/api/Productos");
        }

        public async Task<List<ProductoVentaBuscarDTO>> ListarProducto()
        {
            HttpClient cliente = ApiClient.ObtenerClienteAutenticado();
            return await cliente.GetFromJsonAsync<List<ProductoVentaBuscarDTO>>
                ("Productos") ?? new List<ProductoVentaBuscarDTO>();
        }


        public async Task<List<Productos>> BuscarProducto(string texto)
        {
            return await _httpClient.GetFromJsonAsync<List<Productos>>($"Productos/buscar?texto={texto}");
        }

        public async Task<ProductoCreadoRespuestaDTO?> CrearProducto(ProductoDTOs producto)
        {
            var response = await _httpClient.PostAsJsonAsync("Productos", producto);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }

            return await response.Content.ReadFromJsonAsync<ProductoCreadoRespuestaDTO>();
        }

        public async Task SubirImagen(int idProducto, string rutaImagen)
        {
            var contenido = new MultipartFormDataContent();

            var bytes = File.ReadAllBytes(rutaImagen);

            var archivo = new ByteArrayContent(bytes);
            contenido.Add(archivo, "imagen", Path.GetFileName(rutaImagen));

            var response = await _httpClient.PostAsync($"Productos/{idProducto}/imagen", contenido);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }
        }


    }
}
