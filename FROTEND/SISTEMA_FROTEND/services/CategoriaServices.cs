using SISTEMA_FROTEND.Conexion;
using SISTEMA_FROTEND.DTOs;
using SISTEMA_FROTEND.DTOs.Catalogo;
using SISTEMA_FROTEND.models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace SISTEMA_FROTEND.services
{
    public class CategoriaServices
    {
        private HttpClient Cliente =>
        ApiClient.ObtenerClienteAutenticado();

        public async Task<List<CategoriaDto>> ListarCategorias()
        {
            var response = await Cliente.GetAsync("categoria");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<CategoriaDto>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            ) ?? new List<CategoriaDto>();
        }


        public async Task<List<ProductoCatalogoDTO>> ListarProductoPorCategoria(int id)
        {
            var response = await Cliente.GetAsync($"Categoria/ListarPorCategoria/{id}"
            );

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<ProductoCatalogoDTO>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            ) ?? new List<ProductoCatalogoDTO>();
        }
    }
}
