using API_SISTEMA.Data;
using Microsoft.EntityFrameworkCore;
using API_SISTEMA.Models;
using API_SISTEMA.Dtos;
using API_SISTEMA.Dtos.Ventas;

namespace API_SISTEMA.Services.Ventas
{
    public class BuscarVentaService
    {
        private readonly SistemaDbContext _context;

        public BuscarVentaService(SistemaDbContext context)
        {
            _context = context;
        }


        public async Task<List<VentaBuscarDto>> BuscarVentasClienteCajaActiva(  int idUsuario, int idCliente)
        {
            // Consulta del turno compartido; el responsable de cada operacion se conserva.
            var sesionCaja = await API_SISTEMA.Services.Caja.CajaSesionActual.Consultar(_context);

            if (sesionCaja == null)
                throw new Exception("El usuario no tiene una caja abierta.");

            var ventas = await _context.ventas
                .AsNoTracking()
                .Where(v =>
                    v.id_sesion_caja == sesionCaja.id_sesion_caja &&
                    v.id_cliente == idCliente)
                .Select(v => new VentaBuscarDto
                {
                    id_venta = v.id_ventas,
                    fecha_venta = v.fecha_venta,
                    total = v.total,

                    detalles = v.DetalleVentas
                        .Select(d => new DetalleVentaBuscarDto
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
