using System.ComponentModel.DataAnnotations;
using System.Data;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Caja;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Auditoria;
using API_SISTEMA.Services.Caja;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services;

public class CajaService(SistemaDbContext context, CatalogoAuditoriaService auditoria, CajaSaldoService saldo)
{
    // Consulta administrativa acotada; no expone entidades ni historial de caja.
    public async Task<List<CajaDisponibleDto>> CajasDisponibles(CancellationToken ct = default) =>
        await context.caja.AsNoTracking().Where(c => c.estado).OrderBy(c => c.id_caja).Take(100)
            .Select(c => new CajaDisponibleDto { IdCaja = c.id_caja, Nombre = c.nombre_caja ?? "Caja " + c.id_caja })
            .ToListAsync(ct);

    private static void Validar(object dto, decimal monto)
    {
        // Se valida tambien fuera del controlador para proteger llamadas internas.
        if (dto is null) throw new CajaValidationException("Los datos de caja son obligatorios.");
        var errores = new List<ValidationResult>();
        if (!Validator.TryValidateObject(dto, new ValidationContext(dto), errores, true))
            throw new CajaValidationException(errores[0].ErrorMessage ?? "Datos invalidos.");
        if (monto < 0 || monto > 99999999.99m || decimal.Round(monto, 2) != monto)
            throw new CajaValidationException("El monto debe respetar decimal(10,2), con hasta dos decimales.");
    }

    private async Task ValidarAdministrador(int id, CancellationToken ct)
    {
        // El permiso se comprueba tambien en el servicio; el ID procede del token.
        if (!await context.usuarios.AnyAsync(u => u.id_usuario == id && u.estado && u.rol.estado && u.rol.nombre == Roles.Administrador, ct))
            throw new CajaValidationException("Se requiere un administrador activo.");
    }

    public async Task<SesionCaja> AbrirCaja(AperturaCajaDto dto, int idUsuario, CancellationToken ct = default)
    {
        if (dto is null) throw new CajaValidationException("Los datos son obligatorios.");
        Validar(dto, dto.monto_inicial);
        await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await ValidarAdministrador(idUsuario, ct);
        if (!await context.caja.AnyAsync(c => c.id_caja == dto.id_caja && c.estado, ct))
            throw new CajaValidationException("La caja no existe o esta inactiva.");
        // Solo puede existir un turno abierto para todo el sistema.
        if (await context.sesioncaja.AnyAsync(s => s.fecha_cierre == null, ct))
            throw new CajaValidationException("Ya existe una sesion de caja abierta.");
        var sesion = new SesionCaja
        {
            id_caja = dto.id_caja, id_usuario_apertura = idUsuario, fecha_apertura = DateTime.Now,
            monto_inicial = dto.monto_inicial, monto_esperado = dto.monto_inicial,
            observacion_apertura = dto.observacion?.Trim() ?? string.Empty
        };
        context.sesioncaja.Add(sesion);
        await context.SaveChangesAsync(ct);
        // Auditoria y apertura se confirman juntas; nunca se registra un exito aislado.
        await auditoria.Agregar("CAJA_ABIERTA", "sesion_caja", sesion.id_sesion_caja, null,
            new() { ["idCaja"] = sesion.id_caja, ["idUsuarioApertura"] = idUsuario, ["montoInicial"] = dto.monto_inicial }, ct);
        await context.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return sesion;
    }

    public async Task<SesionCaja> CerrarCaja(CierreCajaDto dto, int idUsuario, CancellationToken ct = default)
    {
        if (dto is null) throw new CajaValidationException("Los datos son obligatorios.");
        Validar(dto, dto.monto_contado);
        await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await ValidarAdministrador(idUsuario, ct);
        var sesion = await CajaSesionActual.ParaOperacion(context, ct);
        // El ID evita que un formulario antiguo cierre accidentalmente un turno nuevo.
        if (sesion.id_sesion_caja != dto.id_sesion_caja)
            throw new CajaValidationException("La sesion indicada ya no es la caja abierta.");
        // Recalcula todos los movimientos bajo el bloqueo del cierre; no confia en el resumen previo del cliente.
        var totales = await saldo.Calcular(sesion.id_sesion_caja, sesion.monto_inicial, ct);
        var esperado = totales.Esperado;
        var diferencia = dto.monto_contado - esperado;
        if (Math.Abs(esperado) > 99999999.99m || Math.Abs(diferencia) > 99999999.99m)
            throw new CajaValidationException("El resultado del cierre supera decimal(10,2).");
        // Se conserva quien abrio y se registra al administrador que realmente cierra.
        sesion.id_usuario_cierre = idUsuario;
        sesion.fecha_cierre = DateTime.Now;
        sesion.monto_esperado = esperado;
        sesion.monto_contado = dto.monto_contado;
        sesion.diferencia = diferencia;
        sesion.observacion_cierre = dto.observacion_cierre?.Trim();
        await auditoria.Agregar("CAJA_CERRADA", "sesion_caja", sesion.id_sesion_caja, null,
            new() { ["idUsuarioApertura"] = sesion.id_usuario_apertura, ["idUsuarioCierre"] = idUsuario,
                ["montoEsperado"] = esperado, ["montoContado"] = dto.monto_contado, ["diferencia"] = diferencia }, ct);
        await context.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return sesion;
    }

    // Solo proyectamos el DTO; las consultas no cargan entidades para editar.
    private IQueryable<ListarSesionesDto> Consulta() => context.sesioncaja.AsNoTracking()
        .OrderByDescending(s => s.id_sesion_caja).Select(s => new ListarSesionesDto
        {
            id_sesion_caja = s.id_sesion_caja, id_caja = s.id_caja, id_usuario_apertura = s.id_usuario_apertura,
            usuario_apertura = s.usuarioapertura.nombre, id_usuario_cierre = s.id_usuario_cierre,
            usuario_cierre = s.usuariocierre == null ? null : s.usuariocierre.nombre,
            fecha_apertura = s.fecha_apertura, fecha_cierre = s.fecha_cierre,
            monto_inicial = s.monto_inicial ?? 0, monto_esperado = s.monto_esperado,
            monto_contado = s.monto_contado, diferencia = s.diferencia,
            observacion_apertura = s.observacion_apertura, observacion_cierre = s.observacion_cierre
        });

    public async Task<ListarSesionesDto?> Actual(CancellationToken ct = default)
    {
        var actual = await CajaSesionActual.Consultar(context, ct);
        return actual is null ? null : await Consulta().SingleAsync(s => s.id_sesion_caja == actual.id_sesion_caja, ct);
    }

    public async Task<List<ListarSesionesDto>> ListarSesionesCaja(int? idCaja = null, int pagina = 1, int tamanoPagina = 50, CancellationToken ct = default)
    {
        // Limites y orden estable evitan lecturas ilimitadas del historial.
        if (idCaja <= 0 || pagina < 1 || pagina > 1000000 || tamanoPagina < 1 || tamanoPagina > 100)
            throw new CajaValidationException("Parametros de consulta invalidos.");
        var consulta = Consulta();
        if (idCaja.HasValue) consulta = consulta.Where(s => s.id_caja == idCaja.Value);
        return await consulta.Skip((pagina - 1) * tamanoPagina).Take(tamanoPagina).ToListAsync(ct);
    }
}
