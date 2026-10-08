using System.Globalization;
using API_SISTEMA.Data;
using API_SISTEMA.Models;
using API_SISTEMA.Security;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Auditoria;

public sealed class PresentacionAuditoriaService(SistemaDbContext context, ContextoPeticion peticion)
{
    // Agrega el evento al contexto; el servicio confirma cambio y evento en su transacción.
    public async Task Agregar(string accion, Presentacion presentacion, object? anterior, object nuevo,
        CancellationToken ct)
    {
        var detalles = AuditoriaDetalles.Crear(anterior, nuevo);
        if (anterior is not null && detalles.Count == 0) return;
        var idUsuario = peticion.IdUsuario ?? throw new InvalidOperationException("La auditoría requiere un usuario autenticado.");
        var usuario = await context.usuarios.AsNoTracking().SingleAsync(u => u.id_usuario == idUsuario, ct);
        context.AuditoriaEventos.Add(new AuditoriaEvento
        {
            FechaUtc = DateTime.UtcNow, IdUsuario = idUsuario,
            UsuarioResponsable = usuario.usuario[..Math.Min(usuario.usuario.Length, 100)],
            Accion = accion, Entidad = "presentaciones",
            IdRegistro = presentacion.IdPresentacion.ToString(CultureInfo.InvariantCulture),
            Resultado = "EXITOSO", Origen = "API", TraceId = peticion.TraceId, DireccionIp = peticion.DireccionIp,
            Detalles = detalles
        });
    }
}
