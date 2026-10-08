using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Gastos;
using API_SISTEMA.Models;
using API_SISTEMA.Services.MovimientoCaja;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens.Experimental;

namespace API_SISTEMA.Services.Gastos
{
    public class CrearGastosService
    {
        private readonly SistemaDbContext _context;
        private readonly MovimientoCajaService _movimientoCajaService;
        public CrearGastosService(SistemaDbContext context, MovimientoCajaService movimientoCajaService)
        {
            _context = context;
            _movimientoCajaService = movimientoCajaService;
        }

        public async Task<int> CrearGasto(IngresarGastoDto gastoDto, int IdUsuario)
        {
            // Pago/gasto y movimiento se confirman antes de permitir el cierre.
            await using var txCaja = await _context.Database.BeginTransactionAsync();
            var turnoCompartido = await API_SISTEMA.Services.Caja.CajaSesionActual.ParaOperacion(_context);
         var sesionCaja = turnoCompartido;
            if (sesionCaja == null)
            {
                throw new Exception("No tienes una sesión de caja abierta.");
            }

            if(gastoDto.monto <= 0)
            {
                throw new Exception("El monto del gasto debe ser mayor a cero.");
            }

            if(gastoDto.descripcion == null || gastoDto.descripcion.Trim() == "")
            {
                throw new Exception("La descripción del gasto no puede estar vacía.");
            }

           
            var gasto = new API_SISTEMA.Models.Gastos
            {
                id_sesion_caja = sesionCaja.id_sesion_caja,
                id_usuario = IdUsuario,
                descripcion = gastoDto.descripcion,
                monto = gastoDto.monto,
                observacion = gastoDto.observacion,
                fecha = DateTime.Now
            };
            _context.gastos.Add(gasto);
            await _context.SaveChangesAsync();

            await _movimientoCajaService.RegistrarMovimiento(
              idSesionCaja: sesionCaja.id_sesion_caja,
              idUsuario: IdUsuario,
              idTipoMovimiento: TiposMovimientoCaja.Gasto,
              monto: (decimal)gasto.monto,
              descripcion: $"Gasto registrado: {gasto.descripcion}",
              idCompra: null,
              idPagoCompra: null
          );
            await _context.SaveChangesAsync();
            await txCaja.CommitAsync();
            return gasto.id_gastos;
        }
    }
}
