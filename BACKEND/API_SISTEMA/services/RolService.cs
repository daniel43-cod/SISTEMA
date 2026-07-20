using API_SISTEMA.data;
using API_SISTEMA.models;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services
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


        public async Task<Rol> CrearRol(Rol rol)
        {
            _context.rols.Add(rol);
            await _context.SaveChangesAsync();
            return rol;
        }


    }
}
