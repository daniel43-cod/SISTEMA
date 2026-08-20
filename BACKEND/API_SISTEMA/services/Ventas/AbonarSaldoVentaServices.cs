using API_SISTEMA.data;
using API_SISTEMA.DTOs.Ventas;
using API_SISTEMA.models;
using API_SISTEMA.services.MovimientoCaja;
using API_SISTEMA.services.PagoCompra;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;


namespace API_SISTEMA.services.Ventas
{
    public class AbonarSaldoVentaServices
    {
        private readonly SistemaDbContext _context;
        private readonly MovimientoCajaService _movimientoCajaService;
        public AbonarSaldoVentaServices(SistemaDbContext context, MovimientoCajaService movimientoCajaService)
        {
            _movimientoCajaService = movimientoCajaService;
            _context = context;
        }

        public async Task<Pagos> AbonarVenta(AbonarSaldoVentaDTO dto, int idUsuario)
        {
            // 1. Buscar la venta
            var venta = await _context.ventas.FirstOrDefaultAsync(v => v.id_ventas == dto.id_venta);

            var sesionCaja = await _context.sesioncaja.FirstOrDefaultAsync(s => s.id_usuario_apertura == idUsuario && s.fecha_cierre == null);


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
            return pagoventa;


        }
    }
}
