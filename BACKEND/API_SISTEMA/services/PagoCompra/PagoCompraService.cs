using System.Data;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Compras;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Compras;
using API_SISTEMA.Services.MovimientoCaja;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.PagoCompra;

public class PagoCompraService(SistemaDbContext context, MovimientoCajaService movimientos,
    API_SISTEMA.Services.Auditoria.CatalogoAuditoriaService auditoria)
{
    public async Task<List<PagoCompraHistorialDto>> ListarPagos(int idCompra, CancellationToken ct)
    {
        if (idCompra <= 0) throw new CompraValidationException("La compra no es válida.");
        if (!await context.registroCompras.AnyAsync(c => c.IdCompra == idCompra, ct))
            throw new CompraValidationException("La compra indicada no existe.");
        return await context.pagosCompras.AsNoTracking().Where(p => p.id_compra == idCompra)
            .OrderByDescending(p => p.fecha_pago).ThenByDescending(p => p.id_pagos_compra)
            .Select(p => new PagoCompraHistorialDto
            {
                IdPago = p.id_pagos_compra, IdCompra = p.id_compra, FechaPago = p.fecha_pago,
                Monto = p.monto, Observacion = p.observacion,
                Usuario = context.usuarios.Where(u => u.id_usuario == p.id_usuario).Select(u => u.usuario).FirstOrDefault() ?? "Sin información"
            }).ToListAsync(ct);
    }

    public async Task<PagosCompra> AbonarCompra(AbonarSaldoCompraDto dto, int idUsuario, CancellationToken ct = default)
    {
        if (dto is null || idUsuario <= 0) throw new CompraValidationException("Solicitud invalida.");
        CrearCompraService.ValidarDto(dto);
        CrearCompraService.ValidarMonto(dto.monto, false);
        if (dto.IdOperacion == Guid.Empty) throw new CompraValidationException("La clave de operación es obligatoria.");
        var existente = await BuscarOperacion(dto, idUsuario, ct);
        if (existente is not null) return existente;
        await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        // El cierre espera a que el abono y su movimiento se confirmen juntos.
        var sesionCaja = await API_SISTEMA.Services.Caja.CajaSesionActual.ParaOperacion(context, ct,
            mensajeCajaCerrada: "Abre la caja para poder abonar la compra.");
        // La caja serializa los abonos. Volver a consultar tras obtener su bloqueo.
        existente = await BuscarOperacion(dto, idUsuario, ct);
        if (existente is not null) { await tx.CommitAsync(ct); return existente; }
        var compra = await context.registroCompras.SingleOrDefaultAsync(c => c.IdCompra == dto.id_compra, ct);
        if (compra is null) throw new CompraValidationException("La compra indicada no existe.");
        if (compra.SaldoPendiente <= 0) throw new CompraValidationException("La compra ya esta pagada.");
        if (dto.monto > compra.SaldoPendiente) throw new CompraValidationException("El abono supera el saldo pendiente.");
        var pago = new PagosCompra
        {
            IdOperacion = dto.IdOperacion,
            id_compra = compra.IdCompra, id_usuario = idUsuario, id_sesion_caja = sesionCaja.id_sesion_caja,
            monto = dto.monto, observacion = dto.observacion?.Trim() ?? string.Empty, fecha_pago = DateTime.Now
        };
        var saldoAnterior = compra.SaldoPendiente;
        context.pagosCompras.Add(pago);
        compra.SaldoPendiente -= dto.monto;
        compra.IdEstadoCompra = compra.SaldoPendiente == 0 ? 1 : 2;
        await context.SaveChangesAsync(ct);
        await movimientos.RegistrarMovimiento(sesionCaja.id_sesion_caja, idUsuario, TiposMovimientoCaja.AbonoCompra,
            dto.monto, $"Abono de compra #{compra.IdCompra}", idCompra: compra.IdCompra, idPagoCompra: pago.id_pagos_compra,
            cancellationToken: ct);
        await auditoria.Agregar("ABONO_COMPRA_REGISTRADO", "pagos_compra", pago.id_pagos_compra, null,
            new Dictionary<string, object?>
            {
                ["id_compra"] = compra.IdCompra, ["id_sesion_caja"] = sesionCaja.id_sesion_caja,
                ["monto"] = pago.monto, ["saldo_anterior"] = saldoAnterior,
                ["saldo_nuevo"] = compra.SaldoPendiente, ["id_estado_compra"] = compra.IdEstadoCompra
            }, ct);
        await context.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return pago;
    }

    private async Task<PagosCompra?> BuscarOperacion(AbonarSaldoCompraDto dto, int idUsuario, CancellationToken ct)
    {
        var pago = await context.pagosCompras.AsNoTracking().SingleOrDefaultAsync(p => p.IdOperacion == dto.IdOperacion, ct);
        if (pago is not null && (pago.id_usuario != idUsuario || pago.id_compra != dto.id_compra ||
            pago.monto != dto.monto || !string.Equals(pago.observacion ?? "", dto.observacion?.Trim() ?? "", StringComparison.Ordinal)))
            throw new CompraOperacionConflictException();
        return pago;
    }
}
