using System.Globalization;
using API_SISTEMA.data;
using API_SISTEMA.models;
using API_SISTEMA.Securyti;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services.Auditoria;

public sealed class AuditoriaService(SistemaDbContext context, ContextoPeticion peticion)
{
    public async Task<bool> RegistrarCierreSesion(CancellationToken cancellationToken = default)
    {
        var id = peticion.IdUsuario;
        if (!id.HasValue) return false;
        var usuario = await context.usuarios.AsNoTracking()
            .SingleOrDefaultAsync(u => u.id_usuario == id.Value, cancellationToken);
        if (usuario is null) return false;
        context.AuditoriaEventos.Add(new AuditoriaEvento
        {
            FechaUtc = DateTime.UtcNow,
            IdUsuario = usuario.id_usuario,
            UsuarioResponsable = usuario.usuario[..Math.Min(usuario.usuario.Length, 100)],
            Accion = "SESION_CERRADA", Entidad = "usuario",
            IdRegistro = usuario.id_usuario.ToString(CultureInfo.InvariantCulture),
            Resultado = "EXITOSO", Origen = "API", Motivo = "Cierre voluntario de sesión.",
            TraceId = peticion.TraceId, DireccionIp = peticion.DireccionIp
        });
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task RegistrarAutenticacionFallida(string identificador,
        CancellationToken cancellationToken = default)
    {
        // El nombre ingresado es la cuenta objetivo, no prueba la identidad del responsable.
        var limpio = new string(identificador.Trim().Where(c => !char.IsControl(c)).Take(100).ToArray());
        context.AuditoriaEventos.Add(new AuditoriaEvento
        {
            FechaUtc = DateTime.UtcNow,
            IdUsuario = null,
            UsuarioResponsable = null,
            IdentificadorIntentado = limpio,
            Accion = "LOGIN_FALLIDO",
            Entidad = "usuario",
            IdRegistro = null,
            Resultado = "RECHAZADO",
            Motivo = "Autenticación rechazada.",
            Origen = "API",
            TraceId = peticion.TraceId,
            DireccionIp = peticion.DireccionIp
        });
        await context.SaveChangesAsync(cancellationToken);
    }

    // Login es anónimo: el responsable proviene de la cuenta validada por LoginService.
    public async Task RegistrarInicioSesion(Usuario usuario, CancellationToken cancellationToken = default)
    {
        context.AuditoriaEventos.Add(new AuditoriaEvento
        {
            FechaUtc = DateTime.UtcNow,
            IdUsuario = usuario.id_usuario,
            UsuarioResponsable = usuario.usuario[..Math.Min(usuario.usuario.Length, 100)],
            Accion = "SESION_INICIADA",
            Entidad = "usuario",
            IdRegistro = usuario.id_usuario.ToString(CultureInfo.InvariantCulture),
            Resultado = "EXITOSO",
            Origen = "API",
            TraceId = peticion.TraceId,
            DireccionIp = peticion.DireccionIp
        });
        await context.SaveChangesAsync(cancellationToken);
    }
}
