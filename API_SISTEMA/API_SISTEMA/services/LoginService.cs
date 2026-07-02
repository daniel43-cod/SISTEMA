using API_SISTEMA.data;
using API_SISTEMA.DTOs;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services
{
    public class LoginService
    {
        
            private readonly SistemaDbContext _context;

            public LoginService(SistemaDbContext context)
            {
                _context = context;
            }


            //hashear las contraseñas mas adelante
            public async Task<LoginRespuestaDTOs?> Login(LoginDTOs dto)
            {
                var usuario = await _context.usuarios
                    .FirstOrDefaultAsync(u => u.usuario == dto.usuario);

                if (usuario == null)
                    return null;

                if (usuario.password != dto.password)
                    return null;

                if (usuario.estado == false)
                    return null;

                return new LoginRespuestaDTOs
                {
                    id_usuario = usuario.id_usuario,
                    nombre = usuario.nombre,
                    rol = usuario.id_rol.ToString(),
                    token = ""
                };
            }


        }
    
}
