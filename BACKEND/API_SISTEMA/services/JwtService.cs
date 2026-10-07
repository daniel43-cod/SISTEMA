using API_SISTEMA.models;
using API_SISTEMA.Utilidades;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;


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

            public string GenerarToken(Usuario usuario, Guid? idSesion = null)
            {
                return GenerarToken(usuario.id_usuario, usuario.nombre, usuario.rol.nombre, "usuario", ObtenerVersion(usuario), idSesion);
            }

            public string GenerarToken(API_SISTEMA.models.CuentaCliente cuenta)
            {
                return GenerarToken(cuenta.IdCuentaCliente, cuenta.CorreoElectronico, "CLIENTE", "cuenta_cliente");
            }

            public string ObtenerVersion(Usuario usuario) => Convert.ToHexString(
                HMACSHA256.HashData(Encoding.UTF8.GetBytes(_jwtSettings.Key),
                    Encoding.UTF8.GetBytes($"{usuario.id_usuario}:{usuario.id_rol}:{usuario.password}")));

            private string GenerarToken(int id, string nombre, string rol, string tipoCuenta, string? version = null, Guid? idSesion = null)
            {
                var claims = new List<Claim>
                {
                  new Claim(JwtRegisteredClaimNames.Sub, id.ToString()),
                  new Claim(JwtRegisteredClaimNames.UniqueName, nombre),
                  new Claim(ClaimTypes.Role, rol),
                  new Claim("tipo_cuenta", tipoCuenta)
            };
            if (version is not null)
                claims.Add(new Claim("version_credencial", version));
            if (idSesion.HasValue)
                claims.Add(new Claim("id_sesion", idSesion.Value.ToString("D")));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Key));

            var credenciales = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(tipoCuenta == "usuario"
                //tiempo de expiracion del tocken
                    ? Math.Min(_jwtSettings.DurationInMinutes, 2) : _jwtSettings.DurationInMinutes),
                signingCredentials: credenciales
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
