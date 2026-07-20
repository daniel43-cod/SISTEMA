using API_SISTEMA.data;
using API_SISTEMA.models;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services
{
    public class RolPermisoService
    {


        private readonly SistemaDbContext _context;

        public RolPermisoService(SistemaDbContext context)
        {
            _context = context;
        }

        public async Task<List<Rol_permisocs>>  ListarRol()
        {
            return await _context.rol_Permisocs.ToListAsync();
        }



        public async Task<Rol_permisocs> CrearRolPermiso(Rol_permisocs rol)
        {

            _context.rol_Permisocs.Add(rol);
            await _context.SaveChangesAsync();
            return rol;
        }


    }
}
