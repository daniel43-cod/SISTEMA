using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Productos;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Productos
{
    public class BuscarCodigoBarraService
    {
        private readonly SistemaDbContext _context;

        public BuscarCodigoBarraService(SistemaDbContext context)
        {
            _context = context;
        }

        public async Task<BuscarCodigoBarraDto?> BuscarPorCodigoBarra(
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
                .Where(p => p.CodigoBarra == codigoBarra)
                .Select(p => new BuscarCodigoBarraDto
                {
                    id_producto = p.IdProducto,
                    codigo_barra = p.CodigoBarra,
                    nombre_producto = p.Nombre,
                    stock = p.Stock ?? 0,

                    presentaciones = p.ProductoPresentaciones
                        .Select(pr => new PresentacionCodigoBarraDto
                        {
                            id_producto_presentacion =
                                pr.id_producto_presentacion,

                            presentacion =
                                pr.Presentacion.Descripcion,

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
