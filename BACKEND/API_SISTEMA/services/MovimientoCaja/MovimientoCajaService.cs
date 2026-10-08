using Microsoft.EntityFrameworkCore;
using API_SISTEMA.Utilidades;
using API_SISTEMA.data;
using API_SISTEMA.models;

namespace API_SISTEMA.services.MovimientoCaja
{
    public class MovimientoCajaService
    {
        private readonly SistemaDbContext _context;
        public MovimientoCajaService(SistemaDbContext context)
        {
            _context = context;
        }


        public async Task RegistrarMovimiento(int idSesionCaja,int idUsuario,int idTipoMovimiento,decimal monto, string descripcion,
        int? idVenta = null,int? idCompra = null,int? idPagoVenta = null,int? idPagoCompra = null)
        {
            // Todo movimiento requiere una transaccion y una sesion que siga abierta.
            if (_context.Database.CurrentTransaction is null)
                throw new InvalidOperationException("El movimiento requiere una transaccion.");
            if (monto <= 0 || monto > 99999999.99m || decimal.Round(monto, 2) != monto)
                throw new CajaValidationException("El monto del movimiento no es valido.");
            var sesion = await API_SISTEMA.services.Caja.CajaSesionActual.ParaOperacion(_context);
            if (sesion.id_sesion_caja != idSesionCaja)
                throw new CajaValidationException("La sesion ya no esta abierta.");

            var movimiento = new models.MovimientoCaja
            {
                id_sesion_caja = idSesionCaja,
                id_usuario = idUsuario,
                id_tipo_movimiento = idTipoMovimiento,
                monto = monto,
                fecha_movimiento = DateTime.Now,
                descripcion = descripcion,

                id_venta = idVenta,
                id_compra = idCompra,
                id_pago_venta = idPagoVenta,
                id_pago_compra = idPagoCompra
            };

            _context.movimientocaja.Add(movimiento);
        }


    }
}
