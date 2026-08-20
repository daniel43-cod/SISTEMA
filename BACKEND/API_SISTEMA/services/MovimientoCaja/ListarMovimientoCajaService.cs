using API_SISTEMA.data;
using API_SISTEMA.DTOs.MovimientoCaja;
using API_SISTEMA.models;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services.MovimientoCaja
{
    public class ListarMovimientoCajaService
    {
        private readonly SistemaDbContext _context;
        public ListarMovimientoCajaService(SistemaDbContext context)
        {
            _context = context;
        }

        public async Task<List<ListarMovimientoCajaDTO>> ListarMovimientos(
    int idUsuario)
        {
            // 1. Buscar la sesión abierta del usuario
            var sesionCaja = await _context.sesioncaja
                .FirstOrDefaultAsync(s =>
                    s.id_usuario_apertura == idUsuario &&
                    s.fecha_cierre == null
                );

            if (sesionCaja == null)
            {
                throw new Exception(
                    "No tienes una sesión de caja abierta."
                );
            }

            // 2. Buscar los movimientos de esa sesión
            var movimientos = await _context.movimientocaja
                .AsNoTracking()
                .Where(m =>
                    m.id_sesion_caja == sesionCaja.id_sesion_caja
                )
                .OrderByDescending(m => m.fecha_movimiento)
                .Select(m => new ListarMovimientoCajaDTO
                {
                    id_movimiento_caja = m.id_movimiento_caja,

                    id_sesion_caja = m.id_sesion_caja,

                    id_tipo_movimiento = m.id_tipo_movimiento,
                    tipo_movimiento = m.tipoMovimientoCaja.nombre_movimiento,
                    naturaleza = m.tipoMovimientoCaja.naturaleza,

                    id_usuario = m.id_usuario,
                    usuario = m.usuario.nombre,

                    fecha_movimiento = m.fecha_movimiento,

                    monto = m.monto,

                    descripcion = m.descripcion,

                    id_venta = m.id_venta,
                    id_compra = m.id_compra,
                    id_pago_venta = m.id_pago_venta,
                    id_pago_compra = m.id_pago_compra
                })
                .ToListAsync();

            return movimientos;
        }
    }
}
