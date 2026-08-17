using API_SISTEMA.data;
using API_SISTEMA.models;
using Microsoft.EntityFrameworkCore;
using API_SISTEMA.models;
using API_SISTEMA.DTOs.Compras;
namespace API_SISTEMA.services.PagoCompra
{
    public class Pago
    {
        private readonly SistemaDbContext _context;

        public Pago(SistemaDbContext context)
        {
            _context = context;
        }

        public async Task<PagosCompra> AbonarCompra(AbonarSaldoCompraDTO dto, int idUsuario)
        {
            // 1. Buscar la compra
            var compra = await _context.registroCompras.FirstOrDefaultAsync(c => c.id_compra == dto.id_compra);

            if (compra == null)
            {
                throw new Exception(
                    "La compra indicada no existe."
                );
            }

            // 2. Validar que todavía tenga saldo
            decimal saldoActual = compra.saldo_pendiente ?? 0m;

            if (saldoActual <= 0)
            {
                throw new Exception(
                    "La compra ya ha sido pagada en su totalidad."
                );
            }

            // 3. Validar monto del abono
            if (dto.monto <= 0)
            {
                throw new Exception(
                    "El monto del abono debe ser mayor que cero."
                );
            }

            if (dto.monto > saldoActual)
            {
                throw new Exception(
                    $"El abono supera el saldo pendiente. " +
                    $"Saldo actual: Q{saldoActual:N2}"
                );
            }

            // 4. Buscar la sesión de caja activa del usuario autenticado
            var sesionCaja = await _context.sesioncaja.FirstOrDefaultAsync(s => s.id_usuario_apertura == idUsuario && s.fecha_cierre == null);

            if (sesionCaja == null)
            {
                throw new Exception(
                    "Debes tener una sesión de caja abierta para registrar el abono."
                );
            }

            // 5. Crear el registro del pago
            var pagoCompra = new PagosCompra
            {
                id_compra = dto.id_compra,
                id_usuario = idUsuario,
                id_sesion_caja = sesionCaja.id_sesion_caja,
                monto = dto.monto,
                observacion = dto.observacion,
                fecha_pago = DateTime.Now
            };

            _context.pagosCompras.Add(pagoCompra);

            // 6. Actualizar saldo pendiente
            compra.saldo_pendiente =
                saldoActual - dto.monto;

            await _context.SaveChangesAsync();

            return pagoCompra;
        }



    }
}
 