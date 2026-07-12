using API_SISTEMA.data;
using API_SISTEMA.DTOs.Compras;
using API_SISTEMA.DTOs.Ventas;
using API_SISTEMA.models;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services
{
    public class CompraService
    {

        private readonly SistemaDbContext _context;

        public CompraService(SistemaDbContext context)
        {
            _context = context;
        }
        
        public async Task<List<ListarComprasDTOs>> listarcompras()
        {
            return await _context.registroCompras.Select(c => new ListarComprasDTOs
            {
                id_compra = c.id_compra,
                id_usuario = c.id_usuario,
                id_empresa = c.id_empresa,
                id_estado_compra = c.id_estado_compra,
                fecha_ingreso = c.fecha_ingreso,
                total_compra = c.total_compra
            }).ToListAsync();
        }

        public async Task<RegistroCompras> CrearCompra(RegistroComprasDTO compraDto)
        {
            if (compraDto == null)
                throw new Exception("La información de la compra es obligatoria.");

            if (compraDto.detalle_compra == null ||
                compraDto.detalle_compra.Count == 0)
            {
                throw new Exception("Debes enviar al menos un detalle de compra.");
            }

            bool usuarioExiste = await _context.usuarios
                .AnyAsync(u => u.id_usuario == compraDto.id_usuario);

            if (!usuarioExiste)
                throw new Exception("El usuario indicado no existe.");

            bool empresaExiste = await _context.empresa
                .AnyAsync(e => e.id_empresa == compraDto.id_empresa);

            if (!empresaExiste)
                throw new Exception("La empresa indicada no existe.");

            bool estadoExiste = await _context.estado_compras
                .AnyAsync(e => e.id_estado_compra == compraDto.id_estado_compra);

            if (!estadoExiste)
                throw new Exception("El estado de compra indicado no existe.");

            decimal totalCompra = 0;

            foreach (var detalleDto in compraDto.detalle_compra)
            {
                if (detalleDto.cantidad <= 0)
                    throw new Exception("La cantidad debe ser mayor a cero.");

                if (detalleDto.precio <= 0)
                    throw new Exception("El precio debe ser mayor a cero.");

                bool productoExiste = await _context.productos
                    .AnyAsync(p => p.id_producto == detalleDto.id_producto);

                if (!productoExiste)
                {
                    throw new Exception(
                        $"El producto con ID {detalleDto.id_producto} no existe.");
                }

                totalCompra += detalleDto.cantidad * detalleDto.precio;
            }

            using var transaccion = await _context.Database.BeginTransactionAsync();

            try
            {
                var compra = new RegistroCompras
                {
                    id_usuario = compraDto.id_usuario,
                    id_empresa = compraDto.id_empresa,
                    id_estado_compra = compraDto.id_estado_compra,
                    fecha_ingreso = DateTime.Now,
                    total_compra = totalCompra
                };

                _context.registroCompras.Add(compra);
                await _context.SaveChangesAsync();

                foreach (var detalleDto in compraDto.detalle_compra)
                {
                    var detalle = new DetalleCompra
                    {
                        id_registro_compra = compra.id_compra,
                        id_producto = detalleDto.id_producto,
                        cantidad = detalleDto.cantidad,
                        precio = detalleDto.precio
                    };

                    _context.detalle_compras.Add(detalle);

                    var producto = await _context.productos
                        .FirstAsync(p =>
                            p.id_producto == detalleDto.id_producto);

                    producto.stock += detalleDto.cantidad;
                }

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                return compra;
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
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
