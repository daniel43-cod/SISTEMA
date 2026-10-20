using System.ComponentModel.DataAnnotations;
using System.Data;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Ventas;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Caja;
using API_SISTEMA.Services.MovimientoCaja;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Ventas;

public class AbonarSaldoVentaService(SistemaDbContext context, MovimientoCajaService movimientos)
{
    public async Task<Pagos> AbonarVenta(AbonarSaldoVentaDto dto, int idUsuario, CancellationToken ct = default)
    {
        if (dto is null || dto.id_venta <= 0) throw new ValidationException("La venta es obligatoria.");
        VentaReglas.Monto(dto.monto, false); VentaReglas.Texto(dto.observacion, 100, "la observación del abono");
        await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var caja = await CajaSesionActual.ParaOperacion(context, ct);
        var venta = await context.ventas.SingleOrDefaultAsync(v => v.id_ventas == dto.id_venta, ct)
            ?? throw new KeyNotFoundException("La venta no existe.");
        var pagado = await context.pagos.Where(p => p.id_venta == dto.id_venta).SumAsync(p => (decimal?)p.monto, ct) ?? 0m;
        var saldo = venta.total - pagado;
        if (saldo <= 0 || dto.monto > saldo) throw new ValidationException("El abono debe ser mayor a cero y no superar el saldo pendiente.");
        var pago = new Pagos { id_venta = venta.id_ventas, id_usuario = idUsuario, id_sesion_caja = caja.id_sesion_caja,
            monto = dto.monto, metodo_pago = "EFECTIVO", observacion = dto.observacion?.Trim(), fecha_pago = DateTime.Now };
        context.pagos.Add(pago);
        venta.monto_pagado = pagado + dto.monto; venta.saldo_pendiente = saldo - dto.monto;
        venta.id_estado_venta = venta.saldo_pendiente == 0 ? 1 : 2;
        await context.SaveChangesAsync(ct);
        await movimientos.RegistrarMovimiento(caja.id_sesion_caja, idUsuario, TiposMovimientoCaja.AbonoVenta, dto.monto,
            $"Abono de venta #{venta.id_ventas}", idVenta: venta.id_ventas, idPagoVenta: pago.id_pago, cancellationToken: ct);
        await context.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return pago;
    }
}
