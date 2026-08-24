using API_SISTEMA.data;
using API_SISTEMA.models;   
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services.Permisos
{
    public class PermisoUsuarioService
    {
        private readonly SistemaDbContext _context;
        public PermisoUsuarioService(SistemaDbContext sistemaDbContext )
        {
            _context = sistemaDbContext;
        }


        public async Task<bool> TienePermiso(int idUsuario, int idPermiso)
        {
            // 1. Validar que el usuario exista
            var usuario = await _context.usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.id_usuario == idUsuario)
                ?? throw new Exception("El usuario autenticado no existe.");

            // 2. Validar que el permiso exista y esté activo
            bool permisoActivo = await _context.tabla_Permisos
                .AsNoTracking()
                .AnyAsync(p => p.id_permiso == idPermiso && p.estado);

            if (!permisoActivo)
            {
                throw new Exception("El permiso no existe o está deshabilitado.");
            }

            // 3. Verificar si existe una excepción directa para el usuario
            var excepcionUsuario = await _context.usuario_permisos
                .AsNoTracking()
                .FirstOrDefaultAsync(up => up.id_usuario == idUsuario && up.id_permiso == idPermiso);

            if (excepcionUsuario != null)
            {
                return excepcionUsuario.permitido;
            }

            // 4. Si no hay excepción, validar contra el rol del usuario
            return await _context.rol_Permisocs
                .AsNoTracking()
                .AnyAsync(rp => rp.id_rol == usuario.id_rol && rp.id_permiso == idPermiso);
        }


    }
}
