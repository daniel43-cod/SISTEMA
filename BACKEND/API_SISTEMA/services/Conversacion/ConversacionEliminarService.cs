using System.ComponentModel.DataAnnotations;
using System.Data;
using API_SISTEMA.Data;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Conversacion;

public sealed class ConversacionEliminarService(SistemaDbContext context)
{
    public async Task<bool> Eliminar(int idCuentaCliente, int idConversacion, CancellationToken ct = default)
    {
        if (idCuentaCliente <= 0 || !await context.CuentaClientes.AsNoTracking()
            .AnyAsync(c => c.IdCuentaCliente == idCuentaCliente && c.Estado, ct))
            throw new UnauthorizedAccessException("La cuenta no está disponible.");
        if (idConversacion <= 0) throw new ValidationException("La conversación no es válida.");

        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var propia = context.Conversaciones.Where(c => c.IdConversacion == idConversacion && c.IdCuentaCliente == idCuentaCliente);
        if (!await propia.AnyAsync(ct)) return false;
        await context.Mensajes.Where(m => m.IdConversacion == idConversacion &&
            m.Conversacion.IdCuentaCliente == idCuentaCliente).ExecuteDeleteAsync(ct);
        var eliminadas = await propia.ExecuteDeleteAsync(ct);
        await transaction.CommitAsync(ct);
        return eliminadas == 1;
    }
}
