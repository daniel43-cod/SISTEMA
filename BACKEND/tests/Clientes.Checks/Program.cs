using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Controllers;
using API_SISTEMA.Data;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Clientes;
using API_SISTEMA.Dtos.Cliente;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Npgsql;

try { await Run(); } catch (Exception e) { Console.Error.WriteLine(e); Environment.ExitCode = 1; }
static async Task Run()
{
    var passed = 0;
    void Check(bool ok, string message) { if (!ok) throw new Exception(message); passed++; }
    async Task Reject<T>(Func<Task> action) where T : Exception
    { try { await action(); } catch (T) { passed++; return; } throw new Exception("Se esperaba " + typeof(T).Name); }
    Check(Attribute.GetCustomAttribute(typeof(ClientesController), typeof(AuthorizeAttribute)) is AuthorizeAttribute a
        && a.Roles == Roles.Administrador, "Controlador exclusivo de administradores");
    var connection = Environment.GetEnvironmentVariable("POSTGRES_TEST_CONNECTION") ?? throw new Exception("Falta conexión aislada.");
    var cs = new NpgsqlConnectionStringBuilder(connection);
    if (cs.Host != "127.0.0.1" || cs.Port != 55438 || cs.Database != "sistema_checks") throw new Exception("Base no autorizada.");
    await using var db = new SistemaDbContext(new DbContextOptionsBuilder<SistemaDbContext>().UseNpgsql(connection).Options);
    await using var tx = await db.Database.BeginTransactionAsync();
    var admin = await db.usuarios.Include(u => u.rol).FirstAsync(u => u.estado && u.rol.estado && u.rol.nombre == Roles.Administrador);
    var sellerRole = await db.Set<Rol>().FirstAsync(r => r.nombre == Roles.Vendedor);
    var seller = new Usuario { nombre = "Historial", apellido = "Vendedor", usuario = "historial-clientes-test", telefono = "123", estado = true, rol = sellerRole };
    db.Add(seller); await db.SaveChangesAsync();
    var service = new HistorialClienteService(db);
    var c = new Cliente { nombre = "PRUEBAHISTCLIENTE", apellido = "Apellido", nit = "NITHIST887", telefono = "TELHIST887", estado = true };
    var empty = new Cliente { nombre = "PRUEBAHISTCLIENTE sin ventas", estado = false };
    var other = new Cliente { nombre = "OTRO cliente", estado = true };
    var box = await db.caja.FirstAsync();
    var closed = new SesionCaja { id_caja = box.id_caja, id_usuario_apertura = admin.id_usuario, id_usuario_cierre = admin.id_usuario,
        fecha_apertura = new(2026, 1, 1), fecha_cierre = new(2026, 1, 2) };
    db.AddRange(c, empty, other, closed); await db.SaveChangesAsync();
    var state = await db.estado_venta.Select(e => e.id_estado_venta).FirstAsync();
    for (var i = 0; i < 32; i++) db.ventas.Add(new Ventas { cliente = c, id_usuario = i % 2 == 0 ? admin.id_usuario : seller.id_usuario,
        sesionCaja = closed, id_estado_venta = state, origen = "LOCAL", fecha_venta = new(2026, 1, 1), subtotal = 100,
        total = 100, monto_pagado = 70, saldo_pendiente = 30 });
    db.ventas.Add(new Ventas { cliente = other, id_usuario = admin.id_usuario, sesionCaja = closed,
        id_estado_venta = state, origen = "LOCAL", total = 900, saldo_pendiente = 900 });
    await db.SaveChangesAsync();
    var list = await service.Listar(admin.id_usuario, new() { Buscar = "pruebAhistcliente", TamanoPagina = 1 });
    Check(list.TotalRegistros == 2 && list.Items.Count == 1 && list.HayMas, "Búsqueda y paginación SQL");
    var next = await service.Listar(admin.id_usuario, new() { Buscar = "PRUEBAHISTCLIENTE", TamanoPagina = 1, Pagina = 2 });
    Check(next.Items.Count == 1 && !next.HayMas && next.Items[0].IdCliente != list.Items[0].IdCliente, "Segunda página");
    foreach (var term in new[] { "NITHIST887", "TELHIST887", "PRUEBAHISTCLIENTE Apellido" })
        Check((await service.Listar(admin.id_usuario, new() { Buscar = term })).Items.Single().IdCliente == c.id_cliente, "Buscar " + term);
    Check((await service.Listar(admin.id_usuario, new() { Buscar = "noexiste887" })).TotalRegistros == 0, "Listado vacío");
    var summary = await service.Resumen(admin.id_usuario, c.id_cliente);
    Check(summary.CantidadVentas == 32 && summary.TotalComprado == 3200 && summary.TotalPagado == 2240 && summary.SaldoPendiente == 960, "Totales completos sin otras personas ni filtro por caja");
    var page = await service.Historial(admin.id_usuario, c.id_cliente, new());
    Check(page.Items.Count == 30 && page.TotalRegistros == 32 && page.HayMas, "30 ventas por defecto de caja cerrada");
    Check(page.Items.Select(v => v.IdUsuario).Distinct().Count() == 2 && page.Items.All(v => v.Responsable.Length > 0), "Todos los responsables");
    Check(page.Items.Select(v => v.IdVenta).SequenceEqual(page.Items.Select(v => v.IdVenta).OrderDescending()), "Desempate estable por ID");
    var last = await service.Historial(admin.id_usuario, c.id_cliente, new() { Pagina = 2 });
    Check(last.Items.Count == 2 && !last.HayMas && !page.Items.Select(v => v.IdVenta).Intersect(last.Items.Select(v => v.IdVenta)).Any(), "Historial sin repetir");
    var zero = await service.Resumen(admin.id_usuario, empty.id_cliente);
    Check(!zero.Estado && zero.CantidadVentas == 0 && zero.TotalComprado == 0 && zero.SaldoPendiente == 0, "Inactivo sin ventas con totales cero");
    Check((await service.Historial(admin.id_usuario, empty.id_cliente, new())).Items.Count == 0, "Historial vacío");
    Check((await service.Historial(admin.id_usuario, c.id_cliente, new() { Pagina = 100 })).Items.Count == 0, "Página posterior al final");
    await Reject<UnauthorizedAccessException>(() => service.Listar(seller.id_usuario, new()));
    await Reject<UnauthorizedAccessException>(() => service.Resumen(seller.id_usuario, c.id_cliente));
    await Reject<UnauthorizedAccessException>(() => service.Historial(seller.id_usuario, c.id_cliente, new()));
    await Reject<UnauthorizedAccessException>(() => service.Listar(int.MaxValue, new()));
    await Reject<KeyNotFoundException>(() => service.Resumen(admin.id_usuario, int.MaxValue));
    await Reject<KeyNotFoundException>(() => service.Historial(admin.id_usuario, int.MaxValue, new()));
    await Reject<ValidationException>(() => service.Resumen(admin.id_usuario, 0));
    await Reject<ValidationException>(() => service.Historial(admin.id_usuario, -1, new()));
    await Reject<ValidationException>(() => service.Listar(admin.id_usuario, new() { Pagina = 0 }));
    await Reject<ValidationException>(() => service.Listar(admin.id_usuario, new() { Buscar = new string('x', 101) }));
    await Reject<ValidationException>(() => service.Historial(admin.id_usuario, c.id_cliente, new() { TamanoPagina = 101 }));
    await Reject<ValidationException>(() => service.Historial(admin.id_usuario, c.id_cliente, new() { TamanoPagina = 0 }));
    admin.estado = false; await db.SaveChangesAsync();
    await Reject<UnauthorizedAccessException>(() => service.Listar(admin.id_usuario, new()));
    await tx.RollbackAsync();
    Console.WriteLine($"{passed} comprobaciones de clientes aprobadas en PostgreSQL; datos de prueba revertidos.");
}
