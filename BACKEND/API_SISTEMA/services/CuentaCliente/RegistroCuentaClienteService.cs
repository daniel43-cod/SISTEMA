using System.ComponentModel.DataAnnotations;
using System.Text;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.RegistroCliente;
using Microsoft.EntityFrameworkCore;
using CuentaClienteModel = API_SISTEMA.Models.CuentaCliente;

namespace API_SISTEMA.Services.CuentaCliente
{
    public class RegistroCuentaClienteService
    {
        private readonly SistemaDbContext dbContext;

        public RegistroCuentaClienteService(SistemaDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<CuentaClienteCreadaDto> Registro(RegistroCuentaClienteDto dto,CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);
            // Reutiliza las reglas del DTO incluso cuando se llama sin controlador.
            Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);

            // BCrypt admite como máximo 72 bytes; no truncar la contraseña.
            if (Encoding.UTF8.GetByteCount(dto.Password) > 72)
            {
                throw new ValidationException("La contraseña no puede superar 72 bytes en UTF-8.");
            }

            //elimina los espacios y convierte todos en minuscula
            var correo = dto.CorreoElectronico.Trim().ToLowerInvariant();
            var clienteExistente = await dbContext.CuentaClientes
                .AnyAsync(c => c.CorreoElectronico.Trim().ToLower() == correo, cancellationToken);

            if (clienteExistente)
            {
                throw new CorreoClienteEnUsoException();
            }

            var cuenta = new CuentaClienteModel
            {
                CorreoElectronico = correo,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Estado = true,
                FechaCreacion = DateTime.UtcNow
            };

            dbContext.CuentaClientes.Add(cuenta);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new CuentaClienteCreadaDto
            {
                IdCuentaCliente = cuenta.IdCuentaCliente,
                CorreoElectronico = cuenta.CorreoElectronico,
                Estado = cuenta.Estado,
                FechaCreacion = cuenta.FechaCreacion
            };
        }
    }
}
