using API_SISTEMA.data;
using API_SISTEMA.DTOs;
using API_SISTEMA.DTOs.Compras;
using API_SISTEMA.DTOs.Ventas;
using API_SISTEMA.models;
using API_SISTEMA.services.MovimientoCaja;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services
{
    public class CompraService
    {

        private readonly SistemaDbContext _context;
        private readonly MovimientoCajaService _movimientoCajaService;
        public CompraService(SistemaDbContext context, MovimientoCajaService movimientoCajaService)
        {
            _context = context;
            _movimientoCajaService = movimientoCajaService;
        }
        
        public async Task<List<ListarComprasDTOs>> listarcompras()
        {
            return await _context.registroCompras.Select(c => new ListarComprasDTOs
            {
                id_compra = c.id_compra,
                id_usuario = c.id_usuario,
                nombre_usuario = c.usuario.nombre,
                id_empresa = c.id_empresa,
                nombre_empresa = c.empresa.nombre_empresa,
                id_estado_compra = c.id_estado_compra,
                descripcion_estado_compra = c.estado_compra.descripcion,
                fecha_ingreso = c.fecha_ingreso,
                total_compra = c.total_compra??0
            }).ToListAsync();
        }


        //LISTAR DETALLE DE COMPRA
        public async Task<List<ListarDetalleCompraDTOs>> ListarDetalleCompra(int id_compra)
        {
            var detalles = await _context.detalle_compras
                .Where(d => d.id_registro_compra == id_compra)
                .Select(d => new ListarDetalleCompraDTOs
                {
                    id_detalle_compra = d.id_detalle_compra,
                    id_registro_compra = d.id_registro_compra,
                    subtotal = d.subtotal,
                    id_producto = d.id_producto,
                    nombre_producto = d.Productos.nombre,
                    cantidad = d.cantidad,
                    precio = d.precio
                })
              
                .ToListAsync();
            return detalles;
        }
    }
}
