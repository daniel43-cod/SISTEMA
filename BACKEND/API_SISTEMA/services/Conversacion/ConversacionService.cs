using System.ComponentModel.DataAnnotations;
using API_SISTEMA.data;
using API_SISTEMA.DTOs.Mensaje;
using API_SISTEMA.services.IA;
using Microsoft.EntityFrameworkCore;
using ConversacionModel = API_SISTEMA.models.Conversacion;
using MensajeModel = API_SISTEMA.models.Mensaje;

namespace API_SISTEMA.services.Conversacion
{
    public class ConversacionService
    {
        private readonly SistemaDbContext _context;
        private readonly OpenAIService _openAI;
        private readonly CatalogoIAService _catalogo;

        public ConversacionService(SistemaDbContext context, OpenAIService openAI, CatalogoIAService catalogo)
        {
            _context = context;
            _openAI = openAI;
            _catalogo = catalogo;
        }

        // El controlador debe obtener idCuentaCliente del JWT, nunca del cuerpo de la petición.
        public async Task<RespuestaMensajeDto> EnviarMensaje(EnviarMensajeDto dto,int idCuentaCliente,CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);
            Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);

            if (idCuentaCliente <= 0 || !await _context.CuentaClientes
                .AnyAsync(c => c.IdCuentaCliente == idCuentaCliente && c.Estado, cancellationToken))
            {
                throw new UnauthorizedAccessException("La cuenta no está disponible.");
            }

            ConversacionModel conversacion;
            if (dto.IdConversacion.HasValue)
            {
                conversacion = await _context.Conversaciones.FirstOrDefaultAsync(
                    c => c.IdConversacion == dto.IdConversacion.Value && c.IdCuentaCliente == idCuentaCliente,
                    cancellationToken)
                    ?? throw new KeyNotFoundException("La conversación no está disponible para esta cuenta.");
            }
            else
            {
                conversacion = new ConversacionModel
                {
                    IdCuentaCliente = idCuentaCliente,
                    IdUsuario = null
                };
            }

            _openAI.ValidarConfiguracion();

            var mensaje = new MensajeModel
            {
                Conversacion = conversacion,
                MensajeRecibido = dto.Mensaje.Trim(),
                Respuesta = null
            };

            // EF inserta la conversación nueva antes del mensaje y asigna sus IDENTITY.
            // Un solo SaveChanges guarda ambos registros en una transacción.
            _context.Mensajes.Add(mensaje);
            await _context.SaveChangesAsync(cancellationToken);

            // Contexto acotado: solo turnos completados de esta conversación y el mensaje actual.
            var anteriores = await _context.Mensajes.AsNoTracking()
                .Where(m => m.IdConversacion == mensaje.IdConversacion &&
                    m.IdMensaje < mensaje.IdMensaje && m.Respuesta != null)
                .OrderByDescending(m => m.IdMensaje).Take(10)
                .Select(m => new { m.MensajeRecibido, m.Respuesta })
                .ToListAsync(cancellationToken);
            var historial = new List<TurnoIA>();
            foreach (var anterior in anteriores.AsEnumerable().Reverse())
            {
                historial.Add(new TurnoIA("user", anterior.MensajeRecibido[..Math.Min(2000, anterior.MensajeRecibido.Length)]));
                historial.Add(new TurnoIA("assistant", anterior.Respuesta![..Math.Min(8000, anterior.Respuesta.Length)]));
            }
            historial.Add(new TurnoIA("user", mensaje.MensajeRecibido));
            try
            {
                mensaje.Respuesta = await _openAI.ResponderAsync(historial, _catalogo.BuscarProductos, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (OpenAIException ex)
            {
                // La pregunta permanece guardada; el cliente recibe sus IDs para continuar el chat.
                ex.IdMensaje = mensaje.IdMensaje;
                ex.IdConversacion = mensaje.IdConversacion;
                throw;
            }

            return new RespuestaMensajeDto
            {
                IdMensaje = mensaje.IdMensaje,
                IdConversacion = mensaje.IdConversacion,
                RespuestaMensaje = mensaje.Respuesta
            };
        }
    }
}
