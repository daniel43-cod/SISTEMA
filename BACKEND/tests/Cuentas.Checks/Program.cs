using API_SISTEMA.Controllers;
using API_SISTEMA.Data;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Cuentas;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.ComponentModel.DataAnnotations;

try { await Run(); } catch (Exception e) { Console.Error.WriteLine(e); Environment.ExitCode = 1; }
static async Task Run()
{
    var passed = 0;
    void Check(bool ok, string name) { if (!ok) throw new Exception(name); passed++; Console.WriteLine("OK " + name); }
    var access = (AuthorizeAttribute?)Attribute.GetCustomAttribute(typeof(CuentasController), typeof(AuthorizeAttribute));
    Check(access?.Roles == Roles.Administrador, "Endpoint exclusivo para administradores");
    var connection = Environment.GetEnvironmentVariable("POSTGRES_TEST_CONNECTION") ?? throw new Exception("Falta conexión de pruebas.");
    var cs = new NpgsqlConnectionStringBuilder(connection);
    if (cs.Host != "127.0.0.1" || cs.Port != 55438 || cs.Database != "sistema_checks") throw new Exception("Solo se permite la base aislada de pruebas.");
    await using var db = new SistemaDbContext(new DbContextOptionsBuilder<SistemaDbContext>().UseNpgsql(connection).Options);
    await using var tx = await db.Database.BeginTransactionAsync();
    var usuario = await db.usuarios.FirstAsync();
    var caja = await db.caja.FirstAsync();
    var cliente = new Cliente { nombre = "CUENTAS prueba", apellido = "Histórico", estado = true };
    var proveedor = new Proveedores { nombre = "CUENTAS proveedor", fecha_creacion = RelojNegocio.Ahora() };
    var cerrada = new SesionCaja { id_caja = caja.id_caja, id_usuario_apertura = usuario.id_usuario, id_usuario_cierre = usuario.id_usuario,
        fecha_apertura = RelojNegocio.Ahora().AddDays(-10), fecha_cierre = RelojNegocio.Ahora().AddDays(-9), monto_inicial = 0 };
    db.AddRange(cliente, proveedor, cerrada); await db.SaveChangesAsync();
    var servicio = new CuentasService(db);
    var antes = await servicio.Consultar(new());
    var estadoVenta = await db.estado_venta.Select(e => e.id_estado_venta).FirstAsync();
    var estadoCompra = await db.estado_compras.Select(e => e.id_estado_compra).FirstAsync();
    Ventas Venta(decimal saldo) => new() { cliente = cliente, id_usuario = usuario.id_usuario, id_sesion_caja = cerrada.id_sesion_caja,
        id_estado_venta = estadoVenta, origen = "LOCAL", total = 100, subtotal = 100, monto_pagado = 100 - saldo, saldo_pendiente = saldo,
        fecha_venta = cerrada.fecha_apertura, ganancia_total = 10 };
    RegistroCompras Compra(decimal saldo, int? sesion) => new() { Proveedores = proveedor, IdUsuario = usuario.id_usuario,
        IdSesionCaja = sesion, IdEstadoCompra = estadoCompra, TotalCompra = 200, SaldoPendiente = saldo, FechaIngreso = cerrada.fecha_apertura };
    var v1 = Venta(60); var v2 = Venta(30); var saldada = Venta(0);
    var c1 = Compra(80, cerrada.id_sesion_caja); var c2 = Compra(40, null); var pagada = Compra(0, cerrada.id_sesion_caja);
    db.AddRange(v1, v2, saldada, c1, c2, pagada); await db.SaveChangesAsync();
    var cobrar = await servicio.Consultar(new() { Buscar = "cuentas", TamanoPagina = 1 });
    Check(cobrar.TotalRegistros == 2 && cobrar.Items.Count == 1 && cobrar.HayMas && cobrar.SaldoFiltrado == 90, "Cobros históricos: total antes de paginar y exclusión de saldadas");
    Check(cobrar.TotalPorCobrar == antes.TotalPorCobrar + 90 && cobrar.TotalPorPagar == antes.TotalPorPagar + 120, "Totales globales de ambas cuentas desde SQL");
    Check(cobrar.Items[0].Nombre == "CUENTAS prueba Histórico", "Nombre completo de cliente");
    var segunda = await servicio.Consultar(new() { Buscar = "cuentas", TamanoPagina = 1, Pagina = 2 });
    Check(segunda.Items.Count == 1 && !segunda.HayMas && segunda.Items[0].Id != cobrar.Items[0].Id, "Paginación estable sin repetir cuentas");
    var pagar = await servicio.Consultar(new() { Tipo = "pagar", Buscar = "CUENTAS proveedor" });
    Check(pagar.Items.Count == 2 && pagar.SaldoFiltrado == 120 && pagar.Items.All(c => c.Id != pagada.IdCompra), "Compras cerradas y antiguas sin sesión, sin pagadas");
    Check((await servicio.Consultar(new() { Buscar = "#" + v1.id_ventas })).Items.Any(c => c.Id == v1.id_ventas), "Búsqueda de venta por número");
    Check((await servicio.Consultar(new() { Tipo = "pagar", Buscar = "#" + c1.IdCompra })).Items.Any(c => c.Id == c1.IdCompra), "Búsqueda de compra por número");
    var vacio = await servicio.Consultar(new() { Buscar = "inexistente-cuentas-987654321" });
    Check(vacio.TotalRegistros == 0 && vacio.SaldoFiltrado == 0 && vacio.Items.Count == 0, "Búsqueda vacía");
    foreach (var filtro in new FiltroCuentas[] { new() { Tipo = "otro" }, new() { Pagina = 0 }, new() { TamanoPagina = 101 }, new() { Buscar = new string('x',101) } })
    {
        try { await servicio.Consultar(filtro); throw new Exception("Se aceptó filtro inválido"); }
        catch (ValidationException) { Check(true, "Filtro inválido rechazado"); }
    }
    v1.saldo_pendiente = 0; v1.monto_pagado = 100; await db.SaveChangesAsync();
    Check((await servicio.Consultar(new() { Buscar = "cuentas" })).TotalRegistros == 1, "Una cuenta saldada desaparece al actualizar");
    await tx.RollbackAsync();
    Console.WriteLine($"{passed} comprobaciones de cuentas aprobadas en PostgreSQL.");
}
