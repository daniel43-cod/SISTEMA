using SISTEMA_FROTEND.Conexion;
using SISTEMA_FROTEND.DTOs.Cliente;
using SISTEMA_FROTEND.DTOs.Compras;
using SISTEMA_FROTEND.DTOs.Productos;
using SISTEMA_FROTEND.models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace SISTEMA_FROTEND.services
{
    public class ProductoService
    {
        private readonly HttpClient _httpClient;
        public ProductoService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:44308/api/");
        }

       public async Task<List<ProductoVentaBuscarDTO>> ListarProducto()
        {
            HttpClient cliente = ApiClient.ObtenerClienteAutenticado();
            return await cliente.GetFromJsonAsync<List<ProductoVentaBuscarDTO>>
                ("Productos/listar") ?? new List<ProductoVentaBuscarDTO>();
        }
            

        public async Task<List<Productos>> BuscarProducto(string texto)
        {
            return await _httpClient.GetFromJsonAsync<List<Productos>>($"Productos/buscar?texto={texto}");
        }


        public async Task<ProductoCreadoRespuestaDTO> CrearProducto(
       ProductoDTOs producto)
        {
            HttpClient cliente =
                ApiClient.ObtenerClienteAutenticado();

            var response = await cliente.PostAsJsonAsync(
                "Productos/crear",
                producto
            );

            string contenido =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Código: {(int)response.StatusCode}\n" +
                    $"Estado: {response.StatusCode}\n" +
                    $"Respuesta API:\n{contenido}"
                );
            }

            var resultado =
                JsonSerializer.Deserialize<ProductoCreadoRespuestaDTO>(
                    contenido,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                );

            return resultado
                ?? throw new Exception(
                    "La API no devolvió una respuesta válida."
                );
        }

        public async Task SubirImagen(int id, string rutaImagen)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rutaImagen))
                    throw new Exception("La ruta de la imagen está vacía.");

                if (!File.Exists(rutaImagen))
                    throw new Exception(
                        $"No se encontró la imagen en la ruta:\n{rutaImagen}"
                    );

                using var contenido = new MultipartFormDataContent();

                var bytes = await File.ReadAllBytesAsync(rutaImagen);

                using var archivo = new ByteArrayContent(bytes);

                contenido.Add(archivo,"imagen",
                    Path.GetFileName(rutaImagen)
                );

                var response = await _httpClient.PostAsync(
                    $"Productos/{id}/imagen",
                    contenido
                );

                string respuesta =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(
                        $"Error al subir la imagen.\n\n" +
                        $"Código HTTP: {(int)response.StatusCode}\n" +
                        $"Estado: {response.StatusCode}\n" +
                        $"Producto: {id}\n" +
                        $"Respuesta de la API:\n{respuesta}"
                    );
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Ocurrió un error al subir la imagen del producto {id}.\n\n" +
                    $"Mensaje: {ex.Message}",
                    ex
                );
            }
        }

    }
}
