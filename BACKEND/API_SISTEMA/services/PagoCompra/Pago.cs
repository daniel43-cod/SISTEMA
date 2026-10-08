using System.Data;
using API_SISTEMA.data;
using API_SISTEMA.DTOs.Compras;
using API_SISTEMA.models;
using API_SISTEMA.services.CompraS;
using API_SISTEMA.services.MovimientoCaja;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services.PagoCompra;

public class Pago(SistemaDbContext context, MovimientoCajaService movimientos)
{
    public async Task<PagosCompra> AbonarCompra(AbonarSaldoCompraDTO dto, int idUsuario, CancellationToken ct = default)
    {
        if (dto is null || idUsuario <= 0) throw new CompraValidationException("Solicitud invalida.");
        CrearCompraService.ValidarDto(dto);
        CrearCompraService.ValidarMonto(dto.monto, false);
        await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var compra = await context.registroCompras.SingleOrDefaultAsync(c => c.IdCompra == dto.id_compra, ct);
        if (compra is null) throw new CompraValidationException("La compra indicada no existe.");
        if (compra.SaldoPendiente <= 0) throw new CompraValidationException("La compra ya esta pagada.");
        if (dto.monto > compra.SaldoPendiente) throw new CompraValidationException("El abono supera el saldo pendiente.");
        var cajas = await context.sesioncaja.Where(s => s.id_usuario_apertura == idUsuario && s.fecha_cierre == null).Take(2).ToListAsync(ct);
        if (cajas.Count != 1) throw new CompraValidationException("Debe existir una sola sesion de caja abierta para registrar el abono.");
        var pago = new PagosCompra
        {
            id_compra = compra.IdCompra, id_usuario = idUsuario, id_sesion_caja = cajas[0].id_sesion_caja,
            monto = dto.monto, observacion = dto.observacion?.Trim() ?? string.Empty, fecha_pago = DateTime.Now
        };
        context.pagosCompras.Add(pago);
        compra.SaldoPendiente -= dto.monto;
        compra.IdEstadoCompra = compra.SaldoPendiente == 0 ? 1 : 2;
        await context.SaveChangesAsync(ct);
        await movimientos.RegistrarMovimiento(cajas[0].id_sesion_caja, idUsuario, TiposMovimientoCaja.AbonoCompra,
            dto.monto, $"Abono de compra #{compra.IdCompra}", idCompra: compra.IdCompra, idPagoCompra: pago.id_pagos_compra);
        await context.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return pago;
    }
}
