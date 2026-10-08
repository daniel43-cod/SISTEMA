using System.ComponentModel.DataAnnotations;
using System.Text;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.RegistroCliente;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.CuentaCliente
{
    public class InicioSesionService
    {
        private readonly SistemaDbContext _context;
        private readonly JwtService _jwtService;

        public InicioSesionService(SistemaDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<InicioSesionClienteRespuestaDto?> IniciarSesion(InicioSesionClienteDto dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);
            Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);
            if (Encoding.UTF8.GetByteCount(dto.Password) > 72)
                return null;

            var correo = dto.CorreoElectronico.Trim().ToLowerInvariant();
            var cuenta = await _context.CuentaClientes.AsNoTracking().FirstOrDefaultAsync(c => c.CorreoElectronico.Trim().ToLower() == correo,cancellationToken);

            if (cuenta == null || !cuenta.Estado)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, cuenta.PasswordHash))
                return null;

            return new InicioSesionClienteRespuestaDto
            {
                IdCuentaCliente = cuenta.IdCuentaCliente,
                CorreoElectronico = cuenta.CorreoElectronico,
                Token = _jwtService.GenerarToken(cuenta)
            };
        }
    }
}
