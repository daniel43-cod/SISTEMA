using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Gastos;
using API_SISTEMA.Services.MovimientoCaja;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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

        public async Task<int> CrearGasto(IngresarGastoDto gastoDto, int idUsuario,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (gastoDto is null) throw new ValidationException("Los datos del gasto son obligatorios.");
            if (idUsuario <= 0) throw new ValidationException("El usuario no es válido.");
            // También valida llamadas al servicio que no pasan por el controlador.
            Validator.ValidateObject(gastoDto, new ValidationContext(gastoDto), validateAllProperties: true);

            // Solo los datos válidos abren transacción y adquieren el bloqueo de caja.
            await using var txCaja = await _context.Database.BeginTransactionAsync(cancellationToken);
            var sesionCaja = await API_SISTEMA.Services.Caja.CajaSesionActual.ParaOperacion(_context, cancellationToken);
            var gasto = new API_SISTEMA.Models.Gastos
            {
                id_sesion_caja = sesionCaja.id_sesion_caja,
                id_usuario = idUsuario,
                descripcion = gastoDto.descripcion.Trim(),
                monto = gastoDto.monto,
                observacion = gastoDto.observacion?.Trim() ?? string.Empty,
                fecha = DateTime.Now
            };
            _context.gastos.Add(gasto);

            await _movimientoCajaService.RegistrarMovimiento(
              idSesionCaja: sesionCaja.id_sesion_caja,
              idUsuario: idUsuario,
              idTipoMovimiento: TiposMovimientoCaja.Gasto,
              monto: gastoDto.monto,
              descripcion: $"Gasto registrado: {gasto.descripcion}",
              idCompra: null,
              idPagoCompra: null,
              cancellationToken: cancellationToken
          );
            // Un solo guardado; si falla o se cancela, la transacción revierte ambos registros.
            await _context.SaveChangesAsync(cancellationToken);
            await txCaja.CommitAsync(cancellationToken);
            return gasto.id_gastos;
        }
    }
}
