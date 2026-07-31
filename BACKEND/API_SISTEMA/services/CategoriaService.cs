using API_SISTEMA.data;
using API_SISTEMA.DTOs;
using API_SISTEMA.DTOs.Catalogo;
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

        public async Task<List<ProductoCatalogoDTOs>> ListarCatalogoPorCategoria(int idCategoria)
        {
            var productos = await _context.productos
                .Where(p => p.id_categoria == idCategoria)
                .Select(p => new ProductoCatalogoDTOs
                {
                    id_producto = p.id_producto,
                    nombre = p.nombre,
                    imagen = p.imagen,
                    stock = p.stock,

                    presentaciones = p.ProductoPresentaciones
                        .Select(pp => new PresentacionCatalogoDTOs
                        {
                            id_producto_presentacion = pp.id_producto_presentacion,
                            presentacion = pp.descripcion,
                            unidades_equivalentes = pp.unidades_equivalentes,
                            precio = pp.precio
                        })
                        .ToList()
                })
                .ToListAsync();

            return productos;
        }
    }
}
