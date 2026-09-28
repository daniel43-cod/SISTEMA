using API_SISTEMA.models;
using API_SISTEMA.Utilidades;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace API_SISTEMA.services
{
    public class JwtService
    {

        private readonly JwtSettings _jwtSettings;

        public JwtService(IOptions<JwtSettings> jwtOptions)
        {
            _jwtSettings = jwtOptions.Value;
        }

        //GENERA LOS TOKENS

            public string GenerarToken(Usuario usuario)
            {
                return GenerarToken(usuario.id_usuario, usuario.nombre, usuario.rol.nombre, "usuario");
            }

            public string GenerarToken(API_SISTEMA.models.CuentaCliente cuenta)
            {
                return GenerarToken(cuenta.IdCuentaCliente, cuenta.CorreoElectronico, "CLIENTE", "cuenta_cliente");
            }

            private string GenerarToken(int id, string nombre, string rol, string tipoCuenta)
            {
                var claims = new[]
                {
                  new Claim(JwtRegisteredClaimNames.Sub, id.ToString()),
                  new Claim(JwtRegisteredClaimNames.UniqueName, nombre),
                  new Claim(ClaimTypes.Role, rol),
                  new Claim("tipo_cuenta", tipoCuenta)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Key));

            var credenciales = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: credenciales
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
