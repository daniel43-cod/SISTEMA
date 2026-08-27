using API_SISTEMA.data;
using API_SISTEMA.DTOs.Productos;
using API_SISTEMA.DTOs.ProductosD;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services.ProductoS
{
    public class BuscarCodigoBarraService
    {
        private readonly SistemaDbContext _context;

        public BuscarCodigoBarraService(SistemaDbContext context)
        {
            _context = context;
        }

        public async Task<BuscarCodigoBarraDTO?> BuscarPorCodigoBarra(
            string codigoBarra)
        {
            if (string.IsNullOrWhiteSpace(codigoBarra))
            {
                throw new Exception(
                    "Debe ingresar un código de barras."
                );
            }

            var producto = await _context.productos
                .AsNoTracking()
                .Where(p => p.codigo_barra == codigoBarra)
                .Select(p => new BuscarCodigoBarraDTO
                {
                    id_producto = p.id_producto,
                    codigo_barra = p.codigo_barra,
                    nombre_producto = p.nombre,
                    stock = p.stock ?? 0,

                    presentaciones = p.ProductoPresentaciones
                        .Select(pr => new PresentacionCodigoBarraDTO
                        {
                            id_producto_presentacion =
                                pr.id_producto_presentacion,

                            presentacion =
                                pr.descripcion,

                            unidades_equivalentes =
                                pr.unidades_equivalentes,

                            precio =
                                pr.precio
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            return producto;
        }

    }
}
