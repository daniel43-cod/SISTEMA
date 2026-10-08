using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Ventas;
using API_SISTEMA.Models;
using API_SISTEMA.Services.MovimientoCaja;
using API_SISTEMA.Services.PagoCompra;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;


namespace API_SISTEMA.Services.Ventas
{
    public class AbonarSaldoVentaService
    {
        private readonly SistemaDbContext _context;
        private readonly MovimientoCajaService _movimientoCajaService;
        public AbonarSaldoVentaService(SistemaDbContext context, MovimientoCajaService movimientoCajaService)
        {
            _movimientoCajaService = movimientoCajaService;
            _context = context;
        }

        public async Task<Pagos> AbonarVenta(AbonarSaldoVentaDto dto, int idUsuario)
        {
            // Pago/gasto y movimiento se confirman antes de permitir el cierre.
            await using var txCaja = await _context.Database.BeginTransactionAsync();
            var turnoCompartido = await API_SISTEMA.Services.Caja.CajaSesionActual.ParaOperacion(_context);
            // 1. Buscar la venta
            var venta = await _context.ventas.FirstOrDefaultAsync(v => v.id_ventas == dto.id_venta);

            var sesionCaja = turnoCompartido;


            if(sesionCaja == null)
            {
                throw new Exception("No hay una sesión de caja abierta para el usuario.");
            }

            if (venta == null)
            {
                throw new Exception(
                    "La venta indicada no existe."
                );
            }

            // 2. Validar que todavía tenga saldo
            decimal saldoActual = venta.saldo_pendiente;

            if (saldoActual <= 0)
            {
                throw new Exception(
                    "La venta ya ha sido pagada en su totalidad."
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

           
         

            // 5. Crear el registro del pago
            var pagoventa = new Pagos
            {
                id_usuario = idUsuario,
                id_sesion_caja = sesionCaja.id_sesion_caja,
                id_venta = dto.id_venta,
                monto = dto.monto,
                fecha_pago = DateTime.Now
            };  
           

            _context.pagos.Add(pagoventa);

            // 6. Actualizar saldo pendiente
            venta.saldo_pendiente = saldoActual - dto.monto;
            await _context.SaveChangesAsync();


            await _movimientoCajaService.RegistrarMovimiento(
                    idSesionCaja: sesionCaja.id_sesion_caja,
                    idUsuario: idUsuario,
                    idTipoMovimiento: TiposMovimientoCaja.AbonoVenta,
                    monto: pagoventa.monto,
                    descripcion: $"Abono de venta #{pagoventa.id_venta}",
                    idVenta: venta.id_ventas,
                    idPagoVenta: pagoventa.id_pago
                );
            await _context.SaveChangesAsync();
            await txCaja.CommitAsync();
            return pagoventa;


        }
    }
}
