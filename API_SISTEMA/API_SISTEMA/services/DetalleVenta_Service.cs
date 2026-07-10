using API_SISTEMA.data;
using API_SISTEMA.DTOs;
using API_SISTEMA.DTOs.Ventas;
using API_SISTEMA.models;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services
{
    public class DetalleVenta_Service
    {
        private readonly SistemaDbContext _context;

        public DetalleVenta_Service(SistemaDbContext context)
        {
            _context = context;

        }

        public async Task<List<ListarDetalleDTOs>> ListarDetalleVenta(int id_venta)
        {
            var detalleVenta = await _context.detalle_Ventas
                .Where(dv => dv.id_venta == id_venta)
                .Select(dv => new ListarDetalleDTOs
                {
                    id_producto = dv.id_producto,
                    nombre_producto = dv.Producto.nombre,
                    descuento = dv.descuento,
                    id_producto_presentacion = dv.id_producto_presentacion,
                    descripcion_resentacion = dv.producto_presentacion.descripcion,
                    cantidad = dv.cantidad,
                })
                .ToListAsync();
            return detalleVenta;
        }

        /*   public async Task<List<DetalleDTOs>> ListarPorVenta(int idVenta)
           {
               return await _context.detalle_Ventas
                   //solo filtra los detalles del idventa ingresada en idVenta
                   .Where(d => d.id_venta == idVenta)
                   .Select(d => new DetalleDTOs
                   {
                       id_detalle_venta = d.id_detalle_venta,
                       id_venta = d.id_venta,
                       id_producto = d.id_producto,
                       nombre_producto = d.Producto.nombre, // si tienes la navegación
                       cantidad = d.cantidad,
                       precio = d.precio,
                       descuento = d.descuento,
                       subtotal = d.subtotal
                   })
                   .ToListAsync();
           }*/


    }
}
