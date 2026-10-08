using System.Data;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Compras;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Compras;
using API_SISTEMA.Services.MovimientoCaja;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.PagoCompra;

public class PagoCompraService(SistemaDbContext context, MovimientoCajaService movimientos)
{
    public async Task<PagosCompra> AbonarCompra(AbonarSaldoCompraDto dto, int idUsuario, CancellationToken ct = default)
    {
        if (dto is null || idUsuario <= 0) throw new CompraValidationException("Solicitud invalida.");
        CrearCompraService.ValidarDto(dto);
        CrearCompraService.ValidarMonto(dto.monto, false);
        await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        // El cierre espera a que el abono y su movimiento se confirmen juntos.
        var sesionCaja = await API_SISTEMA.Services.Caja.CajaSesionActual.ParaOperacion(context, ct);
        var compra = await context.registroCompras.SingleOrDefaultAsync(c => c.IdCompra == dto.id_compra, ct);
        if (compra is null) throw new CompraValidationException("La compra indicada no existe.");
        if (compra.SaldoPendiente <= 0) throw new CompraValidationException("La compra ya esta pagada.");
        if (dto.monto > compra.SaldoPendiente) throw new CompraValidationException("El abono supera el saldo pendiente.");
        var pago = new PagosCompra
        {
            id_compra = compra.IdCompra, id_usuario = idUsuario, id_sesion_caja = sesionCaja.id_sesion_caja,
            monto = dto.monto, observacion = dto.observacion?.Trim() ?? string.Empty, fecha_pago = DateTime.Now
        };
        context.pagosCompras.Add(pago);
        compra.SaldoPendiente -= dto.monto;
        compra.IdEstadoCompra = compra.SaldoPendiente == 0 ? 1 : 2;
        await context.SaveChangesAsync(ct);
        await movimientos.RegistrarMovimiento(sesionCaja.id_sesion_caja, idUsuario, TiposMovimientoCaja.AbonoCompra,
            dto.monto, $"Abono de compra #{compra.IdCompra}", idCompra: compra.IdCompra, idPagoCompra: pago.id_pagos_compra);
        await context.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return pago;
    }
}
