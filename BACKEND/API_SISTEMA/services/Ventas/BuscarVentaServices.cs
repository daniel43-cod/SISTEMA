using API_SISTEMA.data;
using Microsoft.EntityFrameworkCore;
using API_SISTEMA.models;
using API_SISTEMA.DTOs;
using API_SISTEMA.DTOs.Ventas;

namespace API_SISTEMA.services.Ventas
{
    public class BuscarVentaServices
    {
        private readonly SistemaDbContext _context;

        public BuscarVentaServices(SistemaDbContext context)
        {
            _context = context;
        }


        public async Task<List<VentaBuscarDTO>> BuscarVentasClienteCajaActiva(  int idUsuario, int idCliente)
        {
            var sesionCaja = await _context.sesioncaja
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.id_usuario_apertura == idUsuario &&
                    s.fecha_cierre == null);

            if (sesionCaja == null)
                throw new Exception("El usuario no tiene una caja abierta.");

            var ventas = await _context.ventas
                .AsNoTracking()
                .Where(v =>
                    v.id_sesion_caja == sesionCaja.id_sesion_caja &&
                    v.id_cliente == idCliente)
                .Select(v => new VentaBuscarDTO
                {
                    id_venta = v.id_ventas,
                    fecha_venta = v.fecha_venta,
                    total = v.total,

                    detalles = v.DetalleVentas
                        .Select(d => new DetalleVentaBuscarDTO
                        {
                            id_detalle_venta = d.id_detalle_venta,
                            id_producto = d.id_producto,
                            producto = d.Producto.nombre,
                            cantidad = d.cantidad,
                            precio = d.precio,
                            descuento = d.descuento??0,
                            subtotal = d.subtotal
                        })
                        .ToList()
                })
                .OrderByDescending(v => v.fecha_venta)
                .ToListAsync();

            return ventas;
        }
    }
}
