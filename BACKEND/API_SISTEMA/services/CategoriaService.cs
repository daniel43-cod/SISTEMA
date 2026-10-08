using API_SISTEMA.Data;
using API_SISTEMA.Dtos;
using API_SISTEMA.Dtos.Catalogo;
using API_SISTEMA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace API_SISTEMA.Services
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

     
        public async Task<List<ProductoCatalogoDto>> ListarCatalogoPorCategoria(int idCategoria)
        {
            var productos = await _context.productos
                // El producto obtiene su categoría a través de la marca.
                .Where(p => p.Marca.IdCategoria == idCategoria)
                .Select(p => new ProductoCatalogoDto
                {
                    id_producto = p.id_producto,
                    nombre = p.nombre,
                    imagen = p.imagen,
                    stock = p.stock??0,

                    presentaciones = p.ProductoPresentaciones
                        .Select(pp => new PresentacionCatalogoDto
                        {
                            id_producto_presentacion = pp.id_producto_presentacion,
                            presentacion = pp.Presentacion.Descripcion,
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
