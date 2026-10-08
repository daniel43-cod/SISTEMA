using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Compras;
using API_SISTEMA.Services.MovimientoCaja;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services;

public class CompraService(SistemaDbContext context, MovimientoCajaService movimientos)
{
    public async Task<List<ListarComprasDto>> listarcompras(int pagina = 1, int tamanoPagina = 50, CancellationToken ct = default)
    {
        if (pagina < 1 || pagina > 1000000 || tamanoPagina < 1 || tamanoPagina > 100)
            throw new CompraValidationException("Pagina o tamano de pagina fuera de rango.");
        return await context.registroCompras.AsNoTracking().OrderByDescending(c => c.IdCompra)
            .Skip((pagina - 1) * tamanoPagina).Take(tamanoPagina).Select(c => new ListarComprasDto
            {
                id_compra = c.IdCompra, id_usuario = c.IdUsuario, nombre_usuario = c.Usuario.nombre,
                id_proveedor = c.IdProveedor, nombre_proveedor = c.Proveedores.nombre,
                id_estado_compra = c.IdEstadoCompra, descripcion_estado_compra = c.EstadoCompra.descripcion,
                fecha_ingreso = c.FechaIngreso, total_compra = c.TotalCompra, saldo_pendiente = c.SaldoPendiente
            }).ToListAsync(ct);
    }

    public async Task<List<ListarDetalleCompraDto>?> ListarDetalleCompra(int id_compra, CancellationToken ct = default)
    {
        if (id_compra <= 0) throw new CompraValidationException("El ID debe ser mayor que cero.");
        if (!await context.registroCompras.AnyAsync(c => c.IdCompra == id_compra, ct)) return null;
        return await context.detalle_compras.AsNoTracking().Where(d => d.id_registro_compra == id_compra)
            .OrderBy(d => d.id_detalle_compra).Select(d => new ListarDetalleCompraDto
            {
                id_detalle_compra = d.id_detalle_compra, id_registro_compra = d.id_registro_compra,
                subtotal = d.subtotal, id_producto = d.id_producto, nombre_producto = d.Productos.nombre,
                cantidad = d.cantidad, precio = d.precio
            }).ToListAsync(ct);
    }
}
