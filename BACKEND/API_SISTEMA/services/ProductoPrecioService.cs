using API_SISTEMA.data;
using API_SISTEMA.models;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services
{
    public class ProductoPrecioService
    {

        private readonly SistemaDbContext _context;

        public ProductoPrecioService(SistemaDbContext context)
        {
            _context = context;
        }

        public async Task<List<Producto_precio>> ListarProductoPrecio()
        {
            return await _context.producto_precios.ToListAsync();
        }
    }
}
