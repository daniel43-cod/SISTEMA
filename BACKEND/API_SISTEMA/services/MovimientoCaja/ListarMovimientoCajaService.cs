using API_SISTEMA.data;
using API_SISTEMA.DTOs.MovimientoCaja;
using API_SISTEMA.models;
using Microsoft.EntityFrameworkCore;
using System.Data;
using API_SISTEMA.Utilidades;

namespace API_SISTEMA.services.MovimientoCaja
{
    public class ListarMovimientoCajaService
    {
        private readonly SistemaDbContext _context;
        private readonly API_SISTEMA.services.Caja.CajaSaldoService _saldo;
        public ListarMovimientoCajaService(SistemaDbContext context, API_SISTEMA.services.Caja.CajaSaldoService saldo)
        {
            _context = context;
            _saldo = saldo;
        }

        public async Task<ResumenMovimientosCajaDTO?> ConsultarResumen(int idSesionCaja,
            CancellationToken cancellationToken = default)
        {
            if (idSesionCaja <= 0)
                throw new CajaValidationException("El ID de sesion debe ser mayor que cero.");

            // Mantiene resumen y listado coherentes durante esta consulta, incluso si llegan nuevos movimientos.
            await using var transaccion = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, cancellationToken);
            var sesion = await _context.sesioncaja.AsNoTracking()
                .Where(s => s.id_sesion_caja == idSesionCaja)
                .Select(s => new { s.id_sesion_caja, s.monto_inicial })
                .SingleOrDefaultAsync(cancellationToken);
            if (sesion is null) return null;
            // Comparte las reglas de saldo con el cierre, dentro de esta transaccion.
            var totales = await _saldo.Calcular(idSesionCaja, sesion.monto_inicial, cancellationToken);
            var consulta = _context.movimientocaja.AsNoTracking()
                .Where(m => m.id_sesion_caja == idSesionCaja);

            var movimientos = await consulta.OrderByDescending(m => m.fecha_movimiento)
                .ThenByDescending(m => m.id_movimiento_caja).Take(100)
                .Select(m => new ListarMovimientoCajaDTO
                {
                    id_movimiento_caja = m.id_movimiento_caja, id_sesion_caja = m.id_sesion_caja,
                    id_tipo_movimiento = m.id_tipo_movimiento, tipo_movimiento = m.tipoMovimientoCaja.nombre_movimiento,
                    naturaleza = m.tipoMovimientoCaja.naturaleza, id_usuario = m.id_usuario, usuario = m.usuario.nombre,
                    fecha_movimiento = m.fecha_movimiento, monto = m.monto, descripcion = m.descripcion,
                    id_venta = m.id_venta, id_compra = m.id_compra,
                    id_pago_venta = m.id_pago_venta, id_pago_compra = m.id_pago_compra
                }).ToListAsync(cancellationToken);

            var respuesta = new ResumenMovimientosCajaDTO
            {
                id_sesion_caja = sesion.id_sesion_caja, monto_inicial = sesion.monto_inicial!.Value,
                total_entradas = totales.Entradas, total_salidas = totales.Salidas,
                saldo_esperado = sesion.monto_inicial.Value + (totales?.Entradas ?? 0m) - (totales?.Salidas ?? 0m),
                total_movimientos = totales.Cantidad, movimientos = movimientos
            };
            await transaccion.CommitAsync(cancellationToken);
            return respuesta;
        }

    }
}
