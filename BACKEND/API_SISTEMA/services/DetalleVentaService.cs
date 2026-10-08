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

        public async Task<List<ListarDetalleDto>> ListarDetalleVenta(int id_venta)
        {
            var detalleVenta = await _context.detalle_Ventas
                .Where(dv => dv.id_venta == id_venta)
                .Select(dv => new ListarDetalleDto
                {
                    id_producto = dv.id_producto,
                    nombre_producto = dv.Producto.nombre,
                    descuento = dv.descuento??0,
                    id_producto_presentacion = dv.id_producto_presentacion,
                    descripcion_resentacion = dv.producto_presentacion.Presentacion.Descripcion,
                    cantidad = dv.cantidad,
                    precio = dv.producto_presentacion.precio
                })
                .ToListAsync();
            return detalleVenta;
        }
    }
}
