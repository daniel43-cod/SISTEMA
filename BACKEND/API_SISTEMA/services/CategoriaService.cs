using API_SISTEMA.data;
using API_SISTEMA.DTOs;
using API_SISTEMA.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace API_SISTEMA.services
{
    public class CategoriaService
    {
        //guarda la conexion de la base de datos
        private readonly SistemaDbContext _context;

        //revibe la informacion y la almacena   
        public CategoriaService(SistemaDbContext context)
        {
            _context = context;
        }

        //metodo para obtener la categoria
        public async Task<List<CategoriaDto>> ListarCategoria()
        {
            //consulta a la base de datos
            return await _context.categorias
             .Select(c => new CategoriaDto
             {
                 Id = c.id_categoria,
                 Nombre = c.nombre_categoria
             })
             .ToListAsync();
        }
    }
}
