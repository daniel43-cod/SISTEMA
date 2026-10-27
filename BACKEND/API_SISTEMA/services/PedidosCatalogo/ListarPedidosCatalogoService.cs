using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.PedidosCatalogo;
using API_SISTEMA.Models;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.PedidosCatalogo;

public sealed class ListarPedidosCatalogoService(SistemaDbContext context)
{
    public async Task<PedidoCatalogoDetalleDto?> ObtenerDetalle(int idPedido, CancellationToken ct = default)
    {
        if (idPedido <= 0)
            throw new ValidationException("El identificador del pedido debe ser mayor a cero.");
        return await context.PedidosCatalogo.AsNoTracking().Where(p => p.IdPedido == idPedido)
            .Select(p => new PedidoCatalogoDetalleDto
            {
                IdPedido = p.IdPedido, FechaUtc = p.FechaUtc, NombreCliente = p.NombreCliente,
                Telefono = p.Telefono, Direccion = p.Direccion, Referencias = p.Referencias,
                Observacion = p.Observacion, ZonaEntrega = p.ZonaEntrega.Nombre,
                Estado = p.Estado.ToString(), Total = p.Total,
                // Usar la copia histórica del pedido, no el precio o nombre actual del catálogo.
                Detalles = p.Detalles.OrderBy(d => d.IdDetallePedido).Select(d => new DetallePedidoCatalogoDto
                {
                    IdProductoPresentacion = d.IdProductoPresentacion, NombreProducto = d.NombreProducto,
                    NombrePresentacion = d.NombrePresentacion, Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario, Subtotal = d.Subtotal
                }).ToList()
            }).SingleOrDefaultAsync(ct);
    }

    public async Task<PedidosPendientesPaginaDto> ListarPendientes(int pagina = 1, CancellationToken ct = default)
    {
        if (pagina is < 1 or > 100000)
            throw new ValidationException("La página debe estar entre 1 y 100000.");
        const int tamanoPagina = 50;
        var pedidos = await context.PedidosCatalogo.AsNoTracking()
            .Where(p => p.Estado == EstadoPedidoCatalogo.Pendiente)
            .OrderByDescending(p => p.FechaUtc).ThenByDescending(p => p.IdPedido)
            .Skip((pagina - 1) * tamanoPagina).Take(tamanoPagina + 1)
            .Select(p => new PedidoPendienteDto
            {
                IdPedido = p.IdPedido, FechaUtc = p.FechaUtc, NombreCliente = p.NombreCliente,
                Telefono = p.Telefono, Direccion = p.Direccion, Referencias = p.Referencias,
                Observacion = p.Observacion, IdZonaEntrega = p.IdZonaEntrega,
                ZonaEntrega = p.ZonaEntrega.Nombre, Total = p.Total
            }).ToListAsync(ct);
        return new PedidosPendientesPaginaDto
        {
            Pagina = pagina, TamanoPagina = tamanoPagina, HayMas = pedidos.Count > tamanoPagina,
            Pedidos = pedidos.Take(tamanoPagina).ToList()
        };
    }
}
