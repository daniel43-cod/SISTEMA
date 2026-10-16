using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Conversacion;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Conversacion;

public sealed class ConversacionListarService(SistemaDbContext context)
{
    public async Task<List<MensajeChatDto>?> ListarMensajes(int idCuentaCliente, int idConversacion,
        int? antesDe = null, int limite = 50, CancellationToken cancellationToken = default)
    {
        if (idCuentaCliente <= 0 || !await context.CuentaClientes.AsNoTracking()
            .AnyAsync(c => c.IdCuentaCliente == idCuentaCliente && c.Estado, cancellationToken))
            throw new UnauthorizedAccessException("La cuenta no está disponible.");
        if (idConversacion <= 0 || antesDe is <= 0 || limite < 1 || limite > 100)
            throw new ValidationException("Los filtros de mensajes no son válidos.");
        if (!await context.Conversaciones.AsNoTracking().AnyAsync(c =>
            c.IdConversacion == idConversacion && c.IdCuentaCliente == idCuentaCliente, cancellationToken))
            return null;

        var mensajes = await context.Mensajes.AsNoTracking()
            .Where(m => m.IdConversacion == idConversacion && (!antesDe.HasValue || m.IdMensaje < antesDe.Value))
            .OrderByDescending(m => m.IdMensaje).Take(limite)
            .Select(m => new MensajeChatDto
            {
                IdMensaje = m.IdMensaje, Mensaje = m.MensajeRecibido, Respuesta = m.Respuesta
            }).ToListAsync(cancellationToken);
        mensajes.Reverse();
        return mensajes;
    }

    public async Task<List<ChatClienteDto>> ListarChats(int idCuentaCliente, int pagina = 1,
        int tamanoPagina = 25, CancellationToken cancellationToken = default)
    {
        if (idCuentaCliente <= 0 || !await context.CuentaClientes.AsNoTracking()
            .AnyAsync(c => c.IdCuentaCliente == idCuentaCliente && c.Estado, cancellationToken))
            throw new UnauthorizedAccessException("La cuenta no está disponible.");
        if (pagina < 1 || pagina > 100000 || tamanoPagina < 1 || tamanoPagina > 100)
            throw new ValidationException("La página o el tamaño de página no son válidos.");

        // El primer mensaje da título al chat; el último determina el orden de actividad.
        return await context.Conversaciones.AsNoTracking()
            .Where(c => c.IdCuentaCliente == idCuentaCliente &&
                context.Mensajes.Any(m => m.IdConversacion == c.IdConversacion))
            .OrderByDescending(c => context.Mensajes.Where(m => m.IdConversacion == c.IdConversacion)
                .Max(m => (int?)m.IdMensaje))
            .ThenByDescending(c => c.IdConversacion)
            .Skip((pagina - 1) * tamanoPagina).Take(tamanoPagina)
            .Select(c => new ChatClienteDto
            {
                IdConversacion = c.IdConversacion,
                Contenido = context.Mensajes.Where(m => m.IdConversacion == c.IdConversacion)
                    .OrderBy(m => m.IdMensaje)
                    .Select(m => m.MensajeRecibido.Length > 100
                        ? m.MensajeRecibido.Substring(0, 100) + "…" : m.MensajeRecibido)
                    .FirstOrDefault() ?? string.Empty
            }).ToListAsync(cancellationToken);
    }
}
