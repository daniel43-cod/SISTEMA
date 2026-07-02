using API_SISTEMA.data;
using API_SISTEMA.models;
using Microsoft.EntityFrameworkCore;
using BCrypt;
using Microsoft.IdentityModel.Abstractions;

namespace API_SISTEMA.services
{
    public class UsuarioService
    {

        private readonly SistemaDbContext _context;

        public UsuarioService(SistemaDbContext context)
        {
            _context = context;

        }

        public async Task<List<Usuario>> ListarUsuario()
        {
            return await _context.usuarios.ToListAsync();
        }

        public async Task<Usuario> CrearUsuario(Usuario usuario)
        {
            //consulta a la base de datos :V
            var UsuarioExistente = await _context.usuarios.FirstOrDefaultAsync(u => u.usuario == usuario.usuario ||  u.telefono==usuario.telefono);

            if (UsuarioExistente != null)
            {
                if (UsuarioExistente.usuario == usuario.usuario)
                    throw new InvalidOperationException("ya existe un usuario registrado con este nombre de usuario, por favor ingresa otro");

                if (UsuarioExistente.usuario == usuario.correo)
                    throw new InvalidOperationException("ya existe un usuario registrado con este correo por favor intenta con otro Correo Electronico");

                throw new InvalidOperationException("ya existe un usuario registrado con el mismo numero de telefono, por favor ingresa otro numero");
            }


            usuario.password= BCrypt.Net.BCrypt.HashPassword(usuario.password);

            _context.usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

    }


}
