using API_SISTEMA.Data;
using API_SISTEMA.Models;
using API_SISTEMA.Services;
using API_SISTEMA.Services.Ventas;
using API_SISTEMA.Services.Permisos;
using API_SISTEMA.Services.MovimientoCaja;
using API_SISTEMA.Dtos.Ventas;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;

static class AccesoVentasChecks
{
    public static async Task Ejecutar(DbContextOptions<SistemaDbContext> options, Action<bool, string> check)
    {
        await using var db = new CheckDb(options);
        db.usuarios.AddRange(new Usuario { id_usuario = 3, id_rol = 1, nombre = "Vendedor B", apellido = "Prueba", usuario = "vendedor-acceso", telefono = "333", estado = true },
            new Usuario { id_usuario = 4, id_rol = 1, nombre = "Inactivo", apellido = "Prueba", usuario = "inactivo-acceso", telefono = "444", estado = false });
        db.rol_Permisocs.Add(new RolPermiso { id_rol = 2, id_permiso = 1 });
        await db.sesioncaja.Where(c => c.fecha_cierre == null).ExecuteUpdateAsync(s => s.SetProperty(c => c.fecha_cierre, (DateTime?)RelojNegocio.Ahora()));
        var caja = new SesionCaja { id_caja = 1, id_usuario_apertura = 2, fecha_apertura = RelojNegocio.Ahora(), monto_inicial = 100 };
        var producto = new Productos { Nombre = "Producto acceso", IdMarca = 1, Stock = 100, PrecioPromedioCompra = 1 };
        var pr = new ProductoPresentacion { Producto = producto, IdPresentacion = 1, unidades_equivalentes = 1, precio = 10, estado = true };
        db.AddRange(caja, pr); await db.SaveChangesAsync();
        var crear = new CrearVentaService(db, new MovimientoCajaService(db), new PermisoUsuarioService(db));
        CrearVentaDto Dto() => new() { id_operacion = Guid.NewGuid(), id_cliente = 1, detalles = [new() { id_producto = producto.IdProducto, id_producto_presentacion = pr.id_producto_presentacion, cantidad = 2 }] };
        var propia = await crear.CrearVenta(Dto(), 3);
        var ajena = await crear.CrearVenta(Dto(), 2);
        async Task Denegado(Func<Task> action, string name)
        {
            try { await action(); throw new Exception("Se permitió " + name); }
            catch (UnauthorizedAccessException) { check(true, "Acceso: " + name); }
        }
        foreach (var usuario in new[] { 0, 4, int.MaxValue })
            await Denegado(() => AccesoVenta.Validar(db, propia.id_ventas, usuario), "usuario inválido/inactivo " + usuario);
        await AccesoVenta.Validar(db, propia.id_ventas, 3); check(true, "Acceso: vendedor propietario permitido");
        await AccesoVenta.Validar(db, propia.id_ventas, 2); check(true, "Acceso: administrador permitido en venta ajena");
        await Denegado(() => AccesoVenta.Validar(db, ajena.id_ventas, 3), "venta ajena rechazada");
        var detalles = new DetalleVentaService(db);
        var pagos = new PagoService(db);
        check((await detalles.ListarDetalleVenta(propia.id_ventas, 3)).Count == 1, "Acceso: detalle propio");
        check((await detalles.ListarDetalleVenta(propia.id_ventas, 2)).Count == 1, "Acceso: detalle ajeno para admin");
        await Denegado(() => detalles.ListarDetalleVenta(ajena.id_ventas, 3), "detalle ajeno");
        check((await pagos.ListarPorVenta(propia.id_ventas, 3)).Count == 0, "Acceso: historial propio sin pagos");
        check((await pagos.ListarPorVenta(propia.id_ventas, 2)).Count == 0, "Acceso: historial ajeno para admin");
        await Denegado(() => pagos.ListarPorVenta(ajena.id_ventas, 3), "historial ajeno");
        var http = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "3")], "test")) };
        var auditoria = new API_SISTEMA.Services.Auditoria.CatalogoAuditoriaService(db, new API_SISTEMA.Security.ContextoPeticion(new HttpContextAccessor { HttpContext = http }));
        var abonar = new AbonarSaldoVentaService(db, new MovimientoCajaService(db), auditoria);
        var antes = await db.pagos.CountAsync(); var movimientos = await db.movimientocaja.CountAsync(); var stock = producto.Stock;
        await Denegado(() => abonar.AbonarVenta(new() { id_operacion = Guid.NewGuid(), id_venta = ajena.id_ventas, monto = 1 }, 3), "abono ajeno");
        ModificarVentaDto Edit(int cantidad) => new() { id_cliente = 1, detalles = [new() { id_producto = producto.IdProducto, id_producto_presentacion = pr.id_producto_presentacion, cantidad = cantidad }] };
        await Denegado(() => new ActualizarVentaService(db).ModificarVenta(ajena.id_ventas, Edit(1), 3), "edición ajena");
        check(await db.pagos.CountAsync() == antes && await db.movimientocaja.CountAsync() == movimientos && producto.Stock == stock,
            "Acceso: rechazo no altera pagos, caja ni inventario");
        var clave = Guid.NewGuid();
        var dto = new AbonarSaldoVentaDto { id_operacion = clave, id_venta = propia.id_ventas, monto = 2 };
        var pago = await abonar.AbonarVenta(dto, 3);
        check((await abonar.AbonarVenta(dto, 3)).id_pago == pago.id_pago, "Acceso: idempotencia del abono propio conservada");
        await Denegado(() => abonar.AbonarVenta(dto, 1), "reintento ajeno también se autoriza");
        await abonar.AbonarVenta(new() { id_operacion = Guid.NewGuid(), id_venta = propia.id_ventas, monto = 1 }, 2);
        check(propia.saldo_pendiente == 17, "Acceso: administrador puede abonar venta de vendedor");
        await new ActualizarVentaService(db).ModificarVenta(propia.id_ventas, Edit(1), 3);
        check(propia.total == 10 && propia.saldo_pendiente == 7, "Acceso: vendedor puede editar su venta");
        await new ActualizarVentaService(db).ModificarVenta(propia.id_ventas, Edit(2), 2);
        check(propia.total == 20 && propia.saldo_pendiente == 17, "Acceso: administrador puede editar venta ajena");
        var busqueda = new BuscarVentaService(db);
        check((await busqueda.BuscarVentasClienteCajaActiva(3, 1)).All(v => v.id_venta == propia.id_ventas), "Acceso: búsqueda por cliente no filtra ventas ajenas");
        check((await busqueda.BuscarVentasClienteCajaActiva(2, 1)).Count == 2, "Acceso: búsqueda de administrador incluye ambas ventas");
        var controller = new API_SISTEMA.Controllers.DetalleVentaController(detalles) { ControllerContext = new ControllerContext { HttpContext = http } };
        await Denegado(() => controller.ListarDetalle(ajena.id_ventas, default), "controlador identifica al vendedor del JWT");
        var cp = new API_SISTEMA.Controllers.PagosController(pagos) { ControllerContext = new ControllerContext { HttpContext = http } };
        await Denegado(() => cp.ListarPorVenta(ajena.id_ventas), "controlador de pagos identifica al vendedor del JWT");
        var comprobante = new API_SISTEMA.Controllers.ComprobanteController(db) { ControllerContext = new ControllerContext { HttpContext = http } };
        await Denegado(() => comprobante.OpcionesWhatsApp(ajena.id_ventas, default), "opciones de comprobante no revelan venta ajena");
        var ticket = new API_SISTEMA.Services.Comprobantes.TicketPosService(db, new PermisoUsuarioService(db), Microsoft.Extensions.Options.Options.Create(new API_SISTEMA.Services.Fel.InfileOptions()));
        await Denegado(() => ticket.Obtener(ajena.id_ventas, 3, default), "ticket de venta ajena");
        var ctx = new ExceptionContext(new ActionContext(http, new RouteData(), new ActionDescriptor()), []) { Exception = new UnauthorizedAccessException("Solo puedes acceder a tus propias ventas.") };
        new VentaExceptionFilter(NullLogger<VentaExceptionFilter>.Instance).OnException(ctx);
        check(ctx.Result is ObjectResult { StatusCode: 403 }, "Acceso: rechazo se transforma en HTTP 403");
        using var cancel = new CancellationTokenSource(); cancel.Cancel();
        try { await AccesoVenta.Validar(db, propia.id_ventas, 3, cancel.Token); throw new Exception("Se ignoró cancelación"); }
        catch (OperationCanceledException) { check(true, "Acceso: respeta cancelación"); }
    }
}
