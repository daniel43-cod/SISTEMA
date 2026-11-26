using API_SISTEMA.Data;
using API_SISTEMA.Dtos;
using API_SISTEMA.Dtos.Ventas;
using API_SISTEMA.Models;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services
{
    public class DetalleVentaService
    {
        private readonly SistemaDbContext _context;

        public DetalleVentaService(SistemaDbContext context)
        {
            _context = context;

        }

        public async Task<List<ListarDetalleDto>> ListarDetalleVenta(int id_venta, int idUsuario, CancellationToken ct = default)
        {
            await Ventas.AccesoVenta.Validar(_context, id_venta, idUsuario, ct);
            var detalleVenta = await _context.detalle_Ventas
                .Where(dv => dv.id_venta == id_venta)
                .Select(dv => new ListarDetalleDto
                {
                    id_producto = dv.id_producto,
                    id_surtido = dv.id_surtido,
                    nombre_producto = dv.id_surtido.HasValue ? dv.Surtido!.Nombre : dv.Producto!.Nombre,
                    componentes = dv.ComponentesSurtido.Select(c => new ComponenteDetalleVentaDto(c.IdProducto, c.Producto.Nombre, c.Cantidad)).ToList(),
                    descuento = dv.descuento??0,
                    id_producto_presentacion = dv.id_producto_presentacion,
                    descripcion_resentacion = dv.producto_presentacion.Presentacion.Descripcion,
                    cantidad = dv.cantidad,
                    precio = dv.precio
                })
                .ToListAsync(ct);
            return detalleVenta;
        }
    }
}
