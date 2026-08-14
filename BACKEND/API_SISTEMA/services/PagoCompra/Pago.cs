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

        public async Task<PagosCompra> RegistrarPagoCompra( PagosCompraDTOs dto,int idUsuario)
        {
            // 1. Buscar la compra
            var compra = await _context.registroCompras
                .FirstOrDefaultAsync(c =>
                    c.id_compra == dto.id_compra);

            if (compra == null)
            {
                throw new Exception("La compra no existe.");
            }

            // 2. Validar el monto ingresado
            if (dto.monto <= 0)
            {
                throw new Exception(
                    "El monto del pago debe ser mayor que cero."
                );
            }

            // 3. Calcular cuánto se ha pagado de esta compra
            decimal totalPagado = await _context.pagosCompras
                .Where(p => p.id_compra == dto.id_compra)
                .SumAsync(p => (decimal?)p.monto)
                ?? 0m;

            // 4. Calcular saldo pendiente
            decimal saldoPendiente =
                compra.total_compra - totalPagado ?? 0;

            if (saldoPendiente <= 0)
            {
                throw new Exception(
                    "La compra ya está completamente pagada."
                );
            }

            // 5. Evitar que paguen más de lo pendiente
            if (dto.monto > saldoPendiente)
            {
                throw new Exception(
                    $"El pago supera el saldo pendiente. " +
                    $"Saldo actual: Q{saldoPendiente:N2}"
                );
            }

            // 6. Crear el pago
            var nuevoPago = new PagosCompra
            {
                id_compra = dto.id_compra,
                id_usuario = idUsuario,
                monto = dto.monto,
                fecha_pago = DateTime.Now
            };

            _context.pagosCompras.Add(nuevoPago);

            await _context.SaveChangesAsync();

            return nuevoPago;
        }



    }
}
