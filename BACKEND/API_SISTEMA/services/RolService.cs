using API_SISTEMA.Data;
using API_SISTEMA.Models;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services
{
    public class RolService
    {

        private readonly SistemaDbContext _context;

        public RolService(SistemaDbContext context)
        {
            _context = context;
        }

        public async Task<List<Rol>> ListarRol()
        {
            return await _context.rols.ToListAsync();
        }




    }
}
