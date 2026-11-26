using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Compras;
using API_SISTEMA.Dtos.Gastos;
using API_SISTEMA.Services.Compras;
using API_SISTEMA.Services.Gastos;
using API_SISTEMA.Services.MovimientoCaja;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.ComponentModel.DataAnnotations;

try { await Run(); } catch (Exception e) { Console.Error.WriteLine(e); Environment.ExitCode = 1; }
static async Task Run()
{
    var cs = Environment.GetEnvironmentVariable("POSTGRES_TEST_CONNECTION") ?? throw new Exception("Falta conexión de pruebas.");
    var target = new NpgsqlConnectionStringBuilder(cs);
    if (target.Host != "127.0.0.1" || target.Port != 55438 || target.Database != "sistema_checks") throw new Exception("Solo se permite la base aislada de pruebas.");
    var options = new DbContextOptionsBuilder<SistemaDbContext>().UseNpgsql(cs).Options;
    await using var db = new SistemaDbContext(options);
    var passed = 0;
    void Check(bool ok, string name) { if (!ok) throw new Exception(name); passed++; Console.WriteLine("OK " + name); }
    var viejasCompras = await db.registroCompras.CountAsync(); var viejosGastos = await db.gastos.CountAsync();
    await db.Database.MigrateAsync();
    Check(await db.registroCompras.CountAsync() == viejasCompras && await db.gastos.CountAsync() == viejosGastos, "Migración conserva registros históricos");
    var usuario = await db.usuarios.Where(u => u.estado && u.rol.nombre == Roles.Administrador).Select(u => u.id_usuario).FirstAsync();
    var producto = await db.productos.Select(p => p.IdProducto).FirstAsync();
    var proveedor = await db.proveedores.Select(p => p.id_proveedor).FirstAsync();
    var caja = await db.sesioncaja.SingleAsync(s => s.fecha_cierre == null);
    RegistroComprasDto Compra(Guid? id = null) => new() { id_operacion = id ?? Guid.NewGuid(), id_proveedor = proveedor, monto_pagado = 2,
        detalle_compra = [new() { id_producto = producto, cantidad = 2, precio = 10 }] };
    IngresarGastoDto Gasto(Guid? id = null) => new() { id_operacion = id ?? Guid.NewGuid(), descripcion = "Prueba idempotencia", monto = 3, observacion = "Original" };
    async Task<int> Comprar(RegistroComprasDto dto, int? user = null)
    {
        await using var c = new SistemaDbContext(options);
        return (await new CrearCompraService(c, new MovimientoCajaService(c)).CrearCompra(dto, user ?? usuario)).IdCompra;
    }
    async Task<int> Gastar(IngresarGastoDto dto, int? user = null)
    {
        await using var c = new SistemaDbContext(options);
        return await new CrearGastosService(c, new MovimientoCajaService(c)).CrearGasto(dto, user ?? usuario);
    }
    async Task Conflict(Func<Task<int>> f, string name)
    {
        try { await f(); throw new Exception("Se aceptó " + name); }
        catch (Exception e) when (e is CompraOperacionConflictException or VentaConflictoException) { Check(true, name); }
    }
    var stock = await db.productos.Where(p => p.IdProducto == producto).Select(p => p.Stock).SingleAsync();
    var movimientos = await db.movimientocaja.CountAsync(); var pagos = await db.pagosCompras.CountAsync();
    var compra = Compra(); var gasto = Gasto();
    var idCompra = await Comprar(compra); var idGasto = await Gastar(gasto);
    Check(await Comprar(compra) == idCompra && await Gastar(gasto) == idGasto, "Repetir devuelve los mismos identificadores");
    compra.detalle_compra[0].precio = 10.00m; gasto.monto = 3.00m;
    Check(await Comprar(compra) == idCompra && await Gastar(gasto) == idGasto, "Los ceros decimales no cambian la identidad de la solicitud");
    Check(await db.productos.Where(p => p.IdProducto == producto).Select(p => p.Stock).SingleAsync() == stock + 2, "La compra aumenta inventario una sola vez");
    Check(await db.movimientocaja.CountAsync() == movimientos + 2 && await db.pagosCompras.CountAsync() == pagos + 1, "Caja y pago inicial no se duplican");
    compra.detalle_compra[0].precio = 11;
    await Conflict(() => Comprar(compra), "Misma clave y otro importe de compra: conflicto"); compra.detalle_compra[0].precio = 10;
    compra.detalle_compra[0].cantidad = 3;
    await Conflict(() => Comprar(compra), "Misma clave y otra cantidad: conflicto"); compra.detalle_compra[0].cantidad = 2;
    compra.observacion = "Cambio";
    await Conflict(() => Comprar(compra), "Misma clave y otra observación de compra: conflicto"); compra.observacion = null;
    await Conflict(() => Comprar(compra, int.MaxValue), "Misma compra y otro usuario: conflicto");
    gasto.monto = 4; await Conflict(() => Gastar(gasto), "Misma clave y otro monto de gasto: conflicto"); gasto.monto = 3;
    gasto.descripcion = "Cambio"; await Conflict(() => Gastar(gasto), "Misma clave y otra descripción: conflicto"); gasto.descripcion = "Prueba idempotencia";
    gasto.observacion = "Cambio"; await Conflict(() => Gastar(gasto), "Misma clave y otra observación de gasto: conflicto"); gasto.observacion = "Original";
    await Conflict(() => Gastar(gasto, int.MaxValue), "Mismo gasto y otro usuario: conflicto");
    try { await Comprar(Compra(Guid.Empty)); throw new Exception("Compra sin clave aceptada"); } catch (CompraValidationException) { Check(true, "Compra exige clave"); }
    try { await Gastar(Gasto(Guid.Empty)); throw new Exception("Gasto sin clave aceptado"); } catch (ValidationException) { Check(true, "Gasto exige clave"); }
    async Task<int> Retry(Func<Task<int>> f)
    {
        for (var attempt = 0; ; attempt++)
            try { return await f(); }
            catch (Exception e) when (attempt < 4 && e.GetBaseException() is PostgresException { SqlState: PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.UniqueViolation or PostgresErrorCodes.DeadlockDetected }) { }
    }
    var simultanea = Compra(); var gastoSimultaneo = Gasto();
    var idsCompras = await Task.WhenAll(Retry(() => Comprar(simultanea)), Retry(() => Comprar(simultanea)));
    var idsGastos = await Task.WhenAll(Retry(() => Gastar(gastoSimultaneo)), Retry(() => Gastar(gastoSimultaneo)));
    Check(idsCompras[0] == idsCompras[1] && await db.registroCompras.CountAsync(c => c.IdOperacion == simultanea.id_operacion) == 1, "Compras simultáneas: una sola operación");
    Check(idsGastos[0] == idsGastos[1] && await db.gastos.CountAsync(g => g.id_operacion == gastoSimultaneo.id_operacion) == 1, "Gastos simultáneos: una sola operación");
    await db.sesioncaja.Where(c => c.id_sesion_caja == caja.id_sesion_caja).ExecuteUpdateAsync(s => s.SetProperty(c => c.fecha_cierre, (DateTime?)RelojNegocio.Ahora()));
    try { Check(await Comprar(compra) == idCompra && await Gastar(gasto) == idGasto, "Reintento tras cierre de caja recupera original sin nuevos movimientos"); }
    finally { await db.sesioncaja.Where(c => c.id_sesion_caja == caja.id_sesion_caja).ExecuteUpdateAsync(s => s.SetProperty(c => c.fecha_cierre, (DateTime?)null)); }
    var falloCompra = Compra(); var falloGasto = Gasto();
    await db.Database.ExecuteSqlRawAsync("CREATE OR REPLACE FUNCTION public.rechazar_idempotencia_checks() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN RAISE EXCEPTION 'fallo simulado de caja'; END $$; CREATE TRIGGER rechazar_idempotencia_checks BEFORE INSERT ON public.movimiento_caja FOR EACH ROW EXECUTE FUNCTION public.rechazar_idempotencia_checks();");
    try
    {
        try { await Comprar(falloCompra); throw new Exception("No ocurrió fallo de compra"); } catch (DbUpdateException) { }
        try { await Gastar(falloGasto); throw new Exception("No ocurrió fallo de gasto"); } catch (DbUpdateException) { }
        Check(!await db.registroCompras.AnyAsync(c => c.IdOperacion == falloCompra.id_operacion) && !await db.gastos.AnyAsync(g => g.id_operacion == falloGasto.id_operacion), "Fallo de caja revierte registro y clave");
    }
    finally { await db.Database.ExecuteSqlRawAsync("DROP TRIGGER rechazar_idempotencia_checks ON public.movimiento_caja; DROP FUNCTION public.rechazar_idempotencia_checks();"); }
    Check(await Comprar(falloCompra) > 0 && await Gastar(falloGasto) > 0, "La misma clave se puede reintentar después de un rollback");
    await using (var duplicado = new SistemaDbContext(options))
    {
        var original = await duplicado.registroCompras.AsNoTracking().SingleAsync(c => c.IdCompra == idCompra);
        original.IdCompra = 0; duplicado.registroCompras.Add(original);
        try { await duplicado.SaveChangesAsync(); throw new Exception("Índice de compra permitió clave repetida"); }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "UX_registro_compra_id_operacion" }) { Check(true, "PostgreSQL impide compras duplicadas aun insertando directamente"); }
    }
    await using (var duplicado = new SistemaDbContext(options))
    {
        var original = await duplicado.gastos.AsNoTracking().SingleAsync(g => g.id_gastos == idGasto);
        original.id_gastos = 0; duplicado.gastos.Add(original);
        try { await duplicado.SaveChangesAsync(); throw new Exception("Índice de gasto permitió clave repetida"); }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "UX_gastos_id_operacion" }) { Check(true, "PostgreSQL impide gastos duplicados aun insertando directamente"); }
    }
    Console.WriteLine($"{passed} comprobaciones de idempotencia en PostgreSQL aprobadas.");
}
