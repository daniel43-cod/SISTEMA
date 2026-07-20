using API_SISTEMA.data;
using API_SISTEMA.models;
using Microsoft.EntityFrameworkCore;
namespace API_SISTEMA.services
{
    public class PermisoService
    {
        private readonly SistemaDbContext _context;

        public PermisoService(SistemaDbContext context)
        {
            _context = context;
        }

        // Verifica si un usuario (a través de su rol) tiene un permiso específico
        public async Task<bool> TienePermiso(int idRol, string nombrePermiso)
        {
            return await _context.rol_Permisocs
                .Include(rp => rp.Permiso)
                .AnyAsync(rp => rp.id_rol == idRol
                             && rp.Permiso.nombre == nombrePermiso
                             && rp.Permiso.estado == true);
        }

        // Trae TODOS los permisos de un rol (útil para mostrar el menú en WPF)
        public async Task<List<string>> ObtenerPermisosDeRol(int idRol)
        {
            return await _context.rol_Permisocs
                .Include(rp => rp.Permiso)
                .Where(rp => rp.id_rol == idRol && rp.Permiso.estado == true)
                .Select(rp => rp.Permiso.nombre)
                .ToListAsync();
        }

        // Para la pantalla del Administrador: asignar/quitar un permiso a un rol
        public async Task AsignarPermiso(int idRol, int idPermiso)
        {
            bool yaExiste = await _context.rol_Permisocs
                .AnyAsync(rp => rp.id_rol == idRol && rp.id_permiso == idPermiso);

            if (yaExiste)
            {
                throw new InvalidOperationException("El rol ya tiene asignado este permiso.");
            }

            var nuevo = new Rol_permisocs
            {
                id_rol = idRol,
                id_permiso = idPermiso
            };

            _context.rol_Permisocs.Add(nuevo);
            await _context.SaveChangesAsync();
        }

        public async Task QuitarPermiso(int idRol, int idPermiso)
        {
            var existente = await _context.rol_Permisocs
                .FirstOrDefaultAsync(rp => rp.id_rol == idRol && rp.id_permiso == idPermiso);

            if (existente == null)
            {
                throw new InvalidOperationException("El rol no tiene asignado este permiso.");
            }

            _context.rol_Permisocs.Remove(existente);
            await _context.SaveChangesAsync();
        }
    }
}
