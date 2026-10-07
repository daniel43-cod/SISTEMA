using System.Globalization;
using System.Text.Json;
using API_SISTEMA.data;
using API_SISTEMA.models;
using API_SISTEMA.Securyti;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services.Auditoria;

public sealed class PresentacionAuditoriaService(SistemaDbContext context, ContextoPeticion peticion)
{
    // Agrega el evento al contexto; el servicio confirma cambio y evento en su transacción.
    public async Task Agregar(string accion, Presentacion presentacion, object? anterior, object nuevo,
        CancellationToken ct)
    {
        var idUsuario = peticion.IdUsuario ?? throw new InvalidOperationException("La auditoría requiere un usuario autenticado.");
        var usuario = await context.usuarios.AsNoTracking().SingleAsync(u => u.id_usuario == idUsuario, ct);
        context.AuditoriaEventos.Add(new AuditoriaEvento
        {
            FechaUtc = DateTime.UtcNow, IdUsuario = idUsuario,
            UsuarioResponsable = usuario.usuario[..Math.Min(usuario.usuario.Length, 100)],
            Accion = accion, Entidad = "presentaciones",
            IdRegistro = presentacion.IdPresentacion.ToString(CultureInfo.InvariantCulture),
            Resultado = "EXITOSO", Origen = "API", TraceId = peticion.TraceId, DireccionIp = peticion.DireccionIp,
            DatosAnteriores = anterior is null ? null : JsonSerializer.Serialize(anterior),
            DatosNuevos = JsonSerializer.Serialize(nuevo)
        });
    }
}
