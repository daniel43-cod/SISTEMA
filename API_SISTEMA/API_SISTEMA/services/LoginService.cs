using API_SISTEMA.data;
using API_SISTEMA.DTOs.Login;
using API_SISTEMA.models;
using Microsoft.EntityFrameworkCore;


namespace API_SISTEMA.services
{
    public class LoginService
    {
        
            private readonly SistemaDbContext _context;
        private readonly JwtService _jwtService;

        public LoginService(SistemaDbContext context, JwtService jwtService)
            {
                _context = context;
                _jwtService = jwtService;
            }


        //hashear las contraseñas mas adelante
        public async Task<LoginRespuestaDTOs?> Login(LoginDTOs dto)
        {
            var usuario = await _context.usuarios
                .Include(u => u.rol)
                .FirstOrDefaultAsync(u => u.usuario == dto.usuario);

            if (usuario == null)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(dto.password, usuario.password))
                return null;

            if (!usuario.estado)
                return null;

            var token = _jwtService.GenerarToken(usuario);

            return new LoginRespuestaDTOs
            {
                id_usuario = usuario.id_usuario,
                nombre = usuario.nombre,
                rol = usuario.rol.nombre,
                token = token
            };
        }

        public async Task CrearUsuario(CrearCuentaDTOs dto)
        {
            var usuario = new Usuario
            {
                nombre = dto.nombre,
                apellido = dto.apellido,
                usuario = dto.usuario,
                correo = dto.corre_electronico,
                telefono = dto.telefono,
                fecha_Creacion = DateTime.Now,

                // Aquí se hashea la contraseña
                password = BCrypt.Net.BCrypt.HashPassword(dto.password),

                id_rol = dto.id_rol,
            };

            _context.usuarios.Add(usuario);
            await _context.SaveChangesAsync();
        }

    }
    
}
