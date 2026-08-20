using API_SISTEMA.data;
using API_SISTEMA.DTOs.Gastos;
using API_SISTEMA.models;
using API_SISTEMA.services.MovimientoCaja;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens.Experimental;

namespace API_SISTEMA.services.Gastos
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

        public async Task<int> CrearGasto(IngresarGastoDTOs gastoDto, int IdUsuario)
        {
         var sesionCaja = await _context.sesioncaja.FirstOrDefaultAsync(s =>  s.id_usuario_apertura == IdUsuario &&s.fecha_cierre == null);
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

           
            var gasto = new models.Gastos
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
            return gasto.id_gastos;
        }
    }
}
