using System.Globalization;
using API_SISTEMA.Data;
using API_SISTEMA.Models;
using API_SISTEMA.Security;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Auditoria;

public sealed class AuditoriaService(SistemaDbContext context, ContextoPeticion peticion,
    API_SISTEMA.Services.Sesiones.SesionCierreService cierres)
{
    public async Task<bool> RegistrarCierreSesion(CancellationToken cancellationToken = default)
    {
        var id = peticion.IdUsuario;
        if (!id.HasValue) return false;
        if (!peticion.IdSesion.HasValue) return false;
        return await cierres.Cerrar(peticion.IdSesion.Value, id.Value, false, cancellationToken);
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
    public async Task RegistrarInicioSesion(Usuario usuario, SesionUsuario sesion, CancellationToken cancellationToken = default)
    {
        context.SesionesUsuario.Add(sesion);
        context.AuditoriaEventos.Add(new AuditoriaEvento
        {
            FechaUtc = DateTime.UtcNow,
            IdUsuario = usuario.id_usuario,
            UsuarioResponsable = usuario.usuario[..Math.Min(usuario.usuario.Length, 100)],
            Accion = "SESION_INICIADA",
            Detalles = AuditoriaDetalles.Crear(null, new { idSesion = sesion.IdSesion, fechaVencimientoUtc = sesion.FechaVencimiento }),
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
