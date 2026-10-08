using API_SISTEMA.Data;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Caja;



public sealed class CajaSaldoService(SistemaDbContext context)
{
    public async Task<CajaSaldo> Calcular(int idSesionCaja, decimal? montoInicial,
        CancellationToken cancellationToken = default)
    {
        if (idSesionCaja <= 0)
            throw new CajaValidationException("El ID de sesion debe ser mayor que cero.");
        if (montoInicial is null || montoInicial < 0)
            throw new CajaValidationException("La sesion tiene un monto inicial invalido.");
        // El llamador mantiene la transaccion del resumen o del cierre; no usamos saldos almacenados en cache.
        if (context.Database.CurrentTransaction is null)
            throw new InvalidOperationException("El calculo de caja requiere una transaccion activa.");

        // SUM y COUNT se ejecutan en la BD sobre toda la sesion, sin cargar cada movimiento en memoria.
        var totales = await context.movimientocaja.AsNoTracking()
            .Where(m => m.id_sesion_caja == idSesionCaja)
            .Select(m => new { m.monto, naturaleza = m.tipoMovimientoCaja.naturaleza.Trim().ToUpper() })
            .GroupBy(m => 1)
            .Select(grupo => new
            {
                Cantidad = grupo.Count(),
                Entradas = grupo.Sum(m => m.naturaleza == "ENTRADA" || m.naturaleza == "INGRESO" ? m.monto : 0m),
                Salidas = grupo.Sum(m => m.naturaleza == "SALIDA" || m.naturaleza == "EGRESO" ? m.monto : 0m),
                Validos = grupo.Count(m => m.monto > 0 &&
                    (m.naturaleza == "ENTRADA" || m.naturaleza == "INGRESO" ||
                     m.naturaleza == "SALIDA" || m.naturaleza == "EGRESO"))
            }).SingleOrDefaultAsync(cancellationToken);
        // Tanto el resumen como el cierre rechazan datos invalidos en lugar de omitirlos del saldo.
        if (totales is not null && totales.Validos != totales.Cantidad)
            throw new CajaValidationException("Hay movimientos con monto o naturaleza invalida en esta sesion.");
        var entradas = totales?.Entradas ?? 0m;
        var salidas = totales?.Salidas ?? 0m;
        return new CajaSaldo(entradas, salidas, montoInicial.Value + entradas - salidas, totales?.Cantidad ?? 0);
    }
}
