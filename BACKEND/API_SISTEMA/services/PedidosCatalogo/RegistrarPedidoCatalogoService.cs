using System.ComponentModel.DataAnnotations;
using System.Data;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.PedidosCatalogo;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Ventas;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.PedidosCatalogo;

public sealed class PedidoCatalogoConflictoException() : Exception("La clave de operación ya se utilizó con otros datos.");

public sealed class RegistrarPedidoCatalogoService(SistemaDbContext context)
{
    public Task<List<ZonaEntregaPublicaDto>> ListarZonas(CancellationToken ct = default)=> context.ZonasEntrega.AsNoTracking().Where(z => z.Estado)
            .OrderBy(z => z.Nombre).ThenBy(z => z.IdZonaEntrega).Select(z => new ZonaEntregaPublicaDto(z.IdZonaEntrega, z.Nombre, z.MinimoCompra)).ToListAsync(ct);

    public async Task<PedidoCatalogoRegistradoDto> Registrar(RegistrarPedidoCatalogoDto dto, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (dto is null || dto.IdOperacion == Guid.Empty || dto.IdZonaEntrega <= 0)
            throw new ValidationException("Envía una clave de operación y una zona válidas.");
        VentaReglas.Texto(dto.NombreCliente, 150, "el nombre", true);
        Validator.ValidateProperty(dto.Telefono,
            new ValidationContext(dto) { MemberName = nameof(dto.Telefono) });
        VentaReglas.Texto(dto.Direccion, 300, "la dirección", true);
        VentaReglas.Texto(dto.Referencias, 300, "las referencias");
        VentaReglas.Texto(dto.Observacion, 500, "la observación");
        var telefono = dto.Telefono.Trim();
        if (dto.Detalles is null || dto.Detalles.Count is < 1 or > 100 ||
            dto.Detalles.Any(d => d is null || d.IdProductoPresentacion <= 0 || d.Cantidad <= 0))
            throw new ValidationException("Envía entre 1 y 100 presentaciones con cantidades enteras mayores a cero.");
        if (dto.Detalles.Select(d => d.IdProductoPresentacion).Distinct().Count() != dto.Detalles.Count)
            throw new ValidationException("Agrupa cada presentación en una sola línea.");

        await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var anterior = await context.PedidosCatalogo.AsNoTracking().Include(p => p.Detalles)
            .SingleOrDefaultAsync(p => p.IdOperacion == dto.IdOperacion, ct);
        if (anterior is not null)
        {
            if (anterior.NombreCliente != dto.NombreCliente.Trim() || anterior.Telefono != telefono ||
                anterior.Direccion != dto.Direccion.Trim() || anterior.IdZonaEntrega != dto.IdZonaEntrega ||
                anterior.Referencias != dto.Referencias?.Trim() || anterior.Observacion != dto.Observacion?.Trim() ||
                !anterior.Detalles.OrderBy(d => d.IdProductoPresentacion).Select(d => (d.IdProductoPresentacion, d.Cantidad))
                    .SequenceEqual(dto.Detalles.OrderBy(d => d.IdProductoPresentacion).Select(d => (d.IdProductoPresentacion, d.Cantidad))))
                throw new PedidoCatalogoConflictoException();
            await tx.CommitAsync(ct);
            return Respuesta(anterior);
        }
        var zona = await context.ZonasEntrega.AsNoTracking().SingleOrDefaultAsync(z => z.IdZonaEntrega == dto.IdZonaEntrega && z.Estado, ct)
            ?? throw new ValidationException("La zona de entrega no está disponible.");
        var pedido = new PedidoCatalogo
        {
            IdOperacion = dto.IdOperacion, NombreCliente = dto.NombreCliente.Trim(), Telefono = telefono,
            Direccion = dto.Direccion.Trim(), Referencias = dto.Referencias?.Trim(), Observacion = dto.Observacion?.Trim(),
            IdZonaEntrega = zona.IdZonaEntrega, Estado = EstadoPedidoCatalogo.Pendiente
        };
        var calculo = await CalcularDetallesPedido.Calcular(context,
            dto.Detalles.Select(d => new EditarDetallePedidoCatalogoDto
            { IdProductoPresentacion = d.IdProductoPresentacion, Cantidad = d.Cantidad }).ToList(), zona.MinimoCompra, ct);
        pedido.Detalles = calculo.Detalles;
        pedido.Total = calculo.Total;
        context.PedidosCatalogo.Add(pedido);
        await context.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return Respuesta(pedido);
    }

    private static PedidoCatalogoRegistradoDto Respuesta(PedidoCatalogo pedido)
        => new(pedido.IdOperacion, pedido.Total, pedido.Estado.ToString());
}
