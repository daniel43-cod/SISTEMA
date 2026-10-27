using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.PedidosCatalogo;
using API_SISTEMA.Dtos.Ventas;
using API_SISTEMA.Models;
using API_SISTEMA.Security;
using API_SISTEMA.Services.Auditoria;
using API_SISTEMA.Services.Ventas;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.PedidosCatalogo;

public sealed class ConfirmarVentaPedidoService(SistemaDbContext context, ContextoPeticion peticion,
    CrearVentaService ventas, CatalogoAuditoriaService auditoria)
{
    public async Task<object> Confirmar(int idPedido, ConfirmarVentaPedidoDto dto, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (dto is null || idPedido <= 0 || dto.IdOperacion == Guid.Empty)
            throw new ValidationException("Envía un pedido y una clave de operación válidos.");
        VentaReglas.Monto(dto.TotalEsperado, false); VentaReglas.Monto(dto.PagoInicial);
        if (dto.Detalles is null || dto.Detalles.Count is < 1 or > 100 || dto.Detalles.Any(d => d is null || d.Cantidad <= 0 || d.IdProductoPresentacion <= 0) ||
            dto.Detalles.Select(d => d.IdProductoPresentacion).Distinct().Count() != dto.Detalles.Count)
            throw new ValidationException("Revisa los productos y cantidades; no repitas presentaciones.");
        var idUsuario = peticion.IdUsuario ?? throw new UnauthorizedAccessException("Usuario no identificado.");
        if (!await context.usuarios.AnyAsync(u => u.id_usuario == idUsuario && u.estado && u.rol.estado && u.rol.nombre == Roles.Administrador, ct))
            throw new UnauthorizedAccessException("Solo un administrador puede confirmar pedidos.");
        await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await context.PedidosCatalogo.Where(p => p.IdPedido == idPedido)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Estado, p => p.Estado), ct);
        var pedido = await context.PedidosCatalogo.Include(p => p.ZonaEntrega)
            .SingleOrDefaultAsync(p => p.IdPedido == idPedido, ct) ?? throw new KeyNotFoundException("El pedido no existe.");
        // La huella del borrador permite comprobar reintentos después de una respuesta perdida.
        var firma = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new
        { IdPedido = idPedido, IdUsuario = idUsuario, dto.TotalEsperado, dto.PagoInicial,
            Detalles = dto.Detalles.OrderBy(d => d.IdProductoPresentacion).Select(d => new
            { d.IdProductoPresentacion, d.Cantidad, Descuento = d.Descuento.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) }) })));
        if (pedido.Estado == EstadoPedidoCatalogo.ConvertidoAVenta && pedido.IdVenta is int existente)
        {
            var op = await context.VentaOperaciones.AsNoTracking().SingleOrDefaultAsync(o => o.id_operacion == dto.IdOperacion && o.id_venta == existente, ct);
            // Se conserva la firma del borrador en la auditoría, no se altera el contrato de ventas normales.
            var coincide = op is not null && op.id_usuario == idUsuario && await context.AuditoriaEventos.AnyAsync(e =>
                e.Accion == "PEDIDO_CONVERTIDO_VENTA" && e.Entidad == "pedido_catalogo" && e.IdRegistro == idPedido.ToString() &&
                e.Detalles.Any(d => d.Campo == "firma" && d.ValorNuevo == firma), ct);
            if (!coincide) throw new PedidoCatalogoEdicionConflictoException("El pedido ya fue convertido en venta.");
            var anterior = await context.ventas.AsNoTracking().SingleAsync(v => v.id_ventas == existente, ct);
            await tx.CommitAsync(ct);
            return Respuesta(anterior);
        }
        if (pedido.Estado != EstadoPedidoCatalogo.Pendiente || pedido.IdVenta is not null)
            throw new PedidoCatalogoEdicionConflictoException("Solo se pueden confirmar pedidos pendientes.");
        if (!pedido.ZonaEntrega.Estado) throw new ValidationException("La zona del pedido no está activa.");
        if (await context.VentaOperaciones.AnyAsync(o => o.id_operacion == dto.IdOperacion, ct))
            throw new PedidoCatalogoEdicionConflictoException("La clave de operación ya se utilizó. Revisa el resultado anterior.");
        var ids = dto.Detalles.Select(d => d.IdProductoPresentacion).ToArray();
        var precios = await context.producto_presentaciones.AsNoTracking().Where(p => ids.Contains(p.id_producto_presentacion))
            .ToDictionaryAsync(p => p.id_producto_presentacion, ct);
        decimal total = 0;
        var lineas = new List<CrearDetalleVentaDto>();
        foreach (var d in dto.Detalles)
        {
            VentaReglas.Monto(d.Descuento);
            if (!precios.TryGetValue(d.IdProductoPresentacion, out var pr)) throw new ValidationException("Una presentación ya no existe.");
            VentaReglas.Monto(pr.precio, false);
            var neto = pr.precio * d.Cantidad - d.Descuento;
            VentaReglas.Monto(neto, false); total += neto;
            lineas.Add(new() { id_producto = pr.id_producto, id_producto_presentacion = pr.id_producto_presentacion, cantidad = d.Cantidad, descuento = d.Descuento });
        }
        if (total != dto.TotalEsperado) throw new PedidoCatalogoEdicionConflictoException("Los precios cambiaron. Actualiza el catálogo y revisa el total antes de confirmar.");
        if (total <= pedido.ZonaEntrega.MinimoCompra) throw new ValidationException("El total con descuentos debe superar el mínimo de la zona.");
        // Los datos provienen exclusivamente del pedido, nunca del borrador enviado por el navegador.
        var venta = await ventas.CrearVenta(new CrearVentaDto
        {
            id_operacion = dto.IdOperacion, origen = "CATALOGO", observacion = $"Pedido catálogo #{pedido.IdPedido}",
            clienteNuevo = new CrearClienteVentaDto { nombre = pedido.NombreCliente, telefono = pedido.Telefono, direccion = pedido.Direccion },
            detalles = lineas, pago = new CrearPagoVentaDto { monto = dto.PagoInicial, metodo_pago = "EFECTIVO" }
        }, idUsuario, ct);
        pedido.IdVenta = venta.id_ventas; pedido.Estado = EstadoPedidoCatalogo.ConvertidoAVenta;
        await auditoria.Agregar("PEDIDO_CONVERTIDO_VENTA", "pedido_catalogo", idPedido, null,
            new Dictionary<string, object?> { ["id_venta"] = venta.id_ventas, ["total"] = venta.total, ["firma"] = firma }, ct);
        await context.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return Respuesta(venta);
    }

    private static object Respuesta(API_SISTEMA.Models.Ventas venta)
        => new { idVenta = venta.id_ventas, total = venta.total, saldoPendiente = venta.saldo_pendiente };
}
