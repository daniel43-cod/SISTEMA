using API_SISTEMA.Data;
using API_SISTEMA.Models;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services
{
    public class RolPermisoService
    {


        private readonly SistemaDbContext _context;

        public RolPermisoService(SistemaDbContext context)
        {
            _context = context;
        }

        public async Task<List<RolPermiso>>  ListarRol()
        {
            return await _context.rol_Permisocs.ToListAsync();
        }



        public async Task<RolPermiso> CrearRolPermiso(RolPermiso rol)
        {

            _context.rol_Permisocs.Add(rol);
            await _context.SaveChangesAsync();
            return rol;
        }


    }
}
