using API_SISTEMA.Data;
using API_SISTEMA.Models;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services
{
    public class ProductoPrecioService
    {

        private readonly SistemaDbContext _context;

        public ProductoPrecioService(SistemaDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductoPrecio>> ListarProductoPrecio()
        {
            return await _context.producto_precios.ToListAsync();
        }
    }
}
