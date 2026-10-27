using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text.Json;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.PedidosCatalogo;
using API_SISTEMA.Models;
using API_SISTEMA.Security;
using API_SISTEMA.Services.Auditoria;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.PedidosCatalogo;

public sealed class EditarPedidoCatalogoService(SistemaDbContext context, ContextoPeticion peticion, CatalogoAuditoriaService auditoria)
{
    public async Task Editar(int idPedido, EditarPedidoCatalogoDto dto, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (idPedido <= 0 || dto is null) throw new ValidationException("Envía un pedido válido.");
        var idUsuario = peticion.IdUsuario ?? throw new UnauthorizedAccessException("Usuario no identificado.");
        if (!await context.usuarios.AsNoTracking().AnyAsync(u => u.id_usuario == idUsuario && u.estado &&
            u.rol.estado && u.rol.nombre == Roles.Administrador, ct))
            throw new UnauthorizedAccessException("Solo un administrador puede editar pedidos.");
        await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        // Adquiere un bloqueo de escritura antes de leer; no cambia el estado.
        await context.PedidosCatalogo.Where(p => p.IdPedido == idPedido)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Estado, p => p.Estado), ct);
        var pedido = await context.PedidosCatalogo.Include(p => p.Detalles).Include(p => p.ZonaEntrega)
            .SingleOrDefaultAsync(p => p.IdPedido == idPedido, ct) ?? throw new KeyNotFoundException("El pedido no existe.");
        if (pedido.Estado != EstadoPedidoCatalogo.Pendiente || pedido.IdVenta is not null)
            throw new PedidoCatalogoEdicionConflictoException("Solo puedes editar pedidos pendientes que no estén convertidos en venta.");
        if (!pedido.ZonaEntrega.Estado) throw new ValidationException("La zona del pedido ya no está activa.");
        var calculo = await CalcularDetallesPedido.Calcular(context, dto.Detalles, pedido.ZonaEntrega.MinimoCompra, ct);
        var antes = Datos(pedido.Total, pedido.Detalles);
        var despues = Datos(calculo.Total, calculo.Detalles);
        if ((string)antes["detalles"]! == (string)despues["detalles"]! && pedido.Total == calculo.Total)
        {
            await tx.CommitAsync(ct);
            return;
        }
        var nuevos = calculo.Detalles.ToDictionary(d => d.IdProductoPresentacion);
        foreach (var actual in pedido.Detalles.ToArray())
        {
            if (!nuevos.Remove(actual.IdProductoPresentacion, out var nuevo))
            {
                pedido.Detalles.Remove(actual);
                context.DetallesPedidoCatalogo.Remove(actual);
            }
            else
            {
                actual.Cantidad = nuevo.Cantidad; actual.PrecioUnitario = nuevo.PrecioUnitario;
                actual.NombreProducto = nuevo.NombreProducto; actual.NombrePresentacion = nuevo.NombrePresentacion;
            }
        }
        foreach (var nuevo in nuevos.Values) pedido.Detalles.Add(nuevo);
        pedido.Total = calculo.Total;
        await auditoria.Agregar("PEDIDO_CATALOGO_EDITADO", "pedido_catalogo", pedido.IdPedido, antes, despues, ct);
        await context.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    private static Dictionary<string, object?> Datos(decimal total, IEnumerable<DetallePedidoCatalogo> detalles)
        => new()
        {
            ["total"] = total,
            ["detalles"] = JsonSerializer.Serialize(detalles.OrderBy(d => d.IdProductoPresentacion).Select(d => new
            {
                d.IdProductoPresentacion, d.NombreProducto, d.NombrePresentacion, d.Cantidad,
                PrecioUnitario = d.PrecioUnitario.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
            }))
        };
}

public sealed class PedidoCatalogoEdicionConflictoException(string mensaje) : Exception(mensaje);
