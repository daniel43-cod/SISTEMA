using System.Globalization;
using API_SISTEMA.Data;
using API_SISTEMA.Models;
using API_SISTEMA.Security;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Auditoria;

public sealed class CatalogoAuditoriaService(SistemaDbContext context, ContextoPeticion peticion)
{
    // El llamador elige los campos permitidos y confirma el evento en su transacción.
    public async Task Agregar(string accion, string entidad, int id,
        Dictionary<string, object?>? anteriores, Dictionary<string, object?> nuevos, CancellationToken ct)
    {
        if (anteriores is not null)
        {
            var originales = anteriores;
            nuevos = nuevos.Where(c => !Equals(originales[c.Key], c.Value))
                .ToDictionary(c => c.Key, c => c.Value);
            if (nuevos.Count == 0) return;
            anteriores = nuevos.Keys.ToDictionary(c => c, c => originales[c]);
        }
        var idUsuario = peticion.IdUsuario ?? throw new InvalidOperationException("La auditoría requiere un usuario autenticado.");
        var usuario = await context.usuarios.AsNoTracking().SingleAsync(u => u.id_usuario == idUsuario, ct);
        context.AuditoriaEventos.Add(new AuditoriaEvento
        {
            FechaUtc = DateTime.UtcNow, IdUsuario = idUsuario,
            UsuarioResponsable = usuario.usuario[..Math.Min(usuario.usuario.Length, 100)],
            Accion = accion, Entidad = entidad, IdRegistro = id.ToString(CultureInfo.InvariantCulture),
            Resultado = "EXITOSO", Origen = "API", TraceId = peticion.TraceId, DireccionIp = peticion.DireccionIp,
            Detalles = AuditoriaDetalles.Crear(anteriores, nuevos)
        });
    }
}
