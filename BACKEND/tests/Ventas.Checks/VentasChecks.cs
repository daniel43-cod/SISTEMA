using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Ventas;
using API_SISTEMA.Models;
using API_SISTEMA.Services;
using API_SISTEMA.Services.Ventas;
using API_SISTEMA.Services.MovimientoCaja;
using API_SISTEMA.Services.Permisos;
using API_SISTEMA.Utilidades;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

int passed = 0;
void Check(bool condition, string name) { if (!condition) throw new Exception(name); passed++; Console.WriteLine("OK " + name); }
void Reject(Action run, string name) { try { run(); } catch (ValidationException) { Check(true, name); return; } throw new Exception("No rechazado: " + name); }
Productos Producto() => new() { IdProducto = 1, Nombre = "Prueba", Stock = 100, PrecioPromedioCompra = 100m };
API_SISTEMA.Services.Auditoria.CatalogoAuditoriaService Auditoria(SistemaDbContext db)
{
    var http = new Microsoft.AspNetCore.Http.DefaultHttpContext();
    http.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
        [new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1")], "checks"));
    return new(db, new API_SISTEMA.Security.ContextoPeticion(new Microsoft.AspNetCore.Http.HttpContextAccessor { HttpContext = http }));
}
ProductoPresentacion Pr() => new() { id_producto_presentacion = 1, id_producto = 1, unidades_equivalentes = 1, precio = 120m };
Reject(() => VentaReglas.Calcular(Producto(), Pr(), 1, 20m), "No vende al costo con descuento Q20");
Reject(() => VentaReglas.Calcular(Producto(), Pr(), 1, 21m), "No vende debajo del costo");
var correcto = VentaReglas.Calcular(Producto(), Pr(), 1, 19m);
Check(correcto.subtotal == 101m && correcto.ganancia == 1m && correcto.precio == 120m && correcto.unidades_descontadas == 1, "Q19 permitido y fotografía del detalle");
foreach (var costo in new decimal?[] { null, 0, -1 }) { var p = Producto(); p.PrecioPromedioCompra = costo; Reject(() => VentaReglas.Calcular(p, Pr(), 1, 0), "Costo inválido bloqueado"); }
var overflow = Pr(); overflow.unidades_equivalentes = 4;
Reject(() => VentaReglas.Calcular(Producto(), overflow, 1073741824, 0), "Desbordamiento de unidades bloqueado");
var sinStock = Producto(); sinStock.Stock = null;
Reject(() => VentaReglas.Calcular(sinStock, Pr(), 1, 0), "Stock nulo bloqueado");
Reject(() => VentaReglas.Calcular(Producto(), Pr(), 1, 0.001m), "Tres decimales rechazados");

await using var connection = new SqliteConnection("Data Source=:memory:");
await connection.OpenAsync();
var options = new DbContextOptionsBuilder<SistemaDbContext>().UseSqlite(connection).Options;
await using (var db = new CheckDb(options))
{
    await db.Database.EnsureCreatedAsync();
    db.rols.Add(new Rol { id_rol = 1, nombre = "VENDEDOR", descripcion = "Prueba", estado = true });
    db.usuarios.Add(new Usuario { id_usuario = 1, id_rol = 1, nombre = "Usuario", apellido = "Prueba", usuario = "prueba", telefono = "123", estado = true });
    db.tabla_Permisos.Add(new TablaPermiso { id_permiso = 1, nombre = "Vender", descripcion = "Prueba", estado = true });
    db.rol_Permisocs.Add(new RolPermiso { id_rol = 1, id_permiso = 1 });
    db.categorias.Add(new Categoria { IdCategoria = 1, nombreCategoria = "Categoría", Estado = true });
    db.Marcas.Add(new Marca { IdMarca = 1, IdCategoria = 1, Nombre = "Marca", Estado = true });
    db.presentaciones.Add(new Presentacion { IdPresentacion = 1, Descripcion = "Unidad", Estado = true });
    var p = Producto(); p.IdMarca = 1; db.productos.Add(p);
    var pr = Pr(); pr.IdPresentacion = 1; pr.estado = true; db.producto_presentaciones.Add(pr);
    db.cliente.Add(new Cliente { id_cliente = 1, nombre = "Cliente inactivo permitido", estado = false });
    db.estado_venta.AddRange(new EstadoVenta { id_estado_venta = 1, descripcion = "Pagada", estado = true }, new EstadoVenta { id_estado_venta = 2, descripcion = "Pendiente", estado = true });
    db.caja.Add(new Caja { id_caja = 1, nombre_caja = "Caja", estado = true });
    db.sesioncaja.Add(new SesionCaja { id_sesion_caja = 1, id_caja = 1, id_usuario_apertura = 1, fecha_apertura = DateTime.Now, monto_inicial = 0, observacion_apertura = "Prueba" });
    db.tipomovimientocaja.AddRange(new TipoMovimientoCaja { id_tipo_movimiento = TiposMovimientoCaja.Venta, nombre_movimiento = "Venta", naturaleza = "ENTRADA" },
        new TipoMovimientoCaja { id_tipo_movimiento = TiposMovimientoCaja.AbonoVenta, nombre_movimiento = "Abono", naturaleza = "ENTRADA" });
    await db.SaveChangesAsync();
}
await using (var db = new CheckDb(options))
{
    var buscador = new API_SISTEMA.Services.Productos.BuscarCodigoBarraService(db);
    foreach (var codigo in new[] { "", "   ", "12 34", "12\n34", new string('1', 101) })
    {
        try { await buscador.BuscarPorCodigoBarra(codigo); throw new Exception("Código inválido aceptado"); }
        catch (ValidationException) { Check(true, "Código inválido rechazado"); }
    }
    var producto = await db.productos.SingleAsync(p => p.IdProducto == 1);
    producto.CodigoBarra = "001234";
    await db.SaveChangesAsync();
    var encontrado = await buscador.BuscarPorCodigoBarra(" 001234 ");
    Check(encontrado?.id_producto == 1 && encontrado.presentaciones.Count == 1, "Código conserva ceros, limpia extremos y devuelve presentaciones");
    Check(await buscador.BuscarPorCodigoBarra("1234") is null, "Búsqueda exacta no elimina ceros iniciales");
    using var cancel = new CancellationTokenSource();
    cancel.Cancel();
    try { await buscador.BuscarPorCodigoBarra("001234", cancel.Token); throw new Exception("Consulta cancelada ejecutada"); }
    catch (OperationCanceledException) { Check(true, "Búsqueda acepta cancelación"); }
    async Task NoDisponible(string mensaje)
    {
        try { await buscador.BuscarPorCodigoBarra("001234"); throw new Exception("Producto no vendible aceptado"); }
        catch (ValidationException ex) { Check(ex.Message == mensaje, mensaje); }
    }
    producto.Stock = 0; await db.SaveChangesAsync();
    await NoDisponible("Producto agotado.");
    producto.Stock = 100; producto.PrecioPromedioCompra = null; await db.SaveChangesAsync();
    await NoDisponible("Sin costo promedio registrado.");
    producto.PrecioPromedioCompra = 100;
    var marca = await db.Marcas.SingleAsync();
    marca.Estado = false; await db.SaveChangesAsync();
    await NoDisponible("Marca desactivada.");
    marca.Estado = true;
    var categoria = await db.categorias.SingleAsync();
    categoria.Estado = false; await db.SaveChangesAsync();
    await NoDisponible("Categoría desactivada.");
    categoria.Estado = true;
    var presentacion = await db.producto_presentaciones.SingleAsync();
    presentacion.estado = false; await db.SaveChangesAsync();
    await NoDisponible("Presentación desactivada.");
    presentacion.estado = true;
    var tipoPresentacion = await db.presentaciones.SingleAsync();
    tipoPresentacion.Estado = false; await db.SaveChangesAsync();
    await NoDisponible("Presentación desactivada.");
    tipoPresentacion.Estado = true;
    presentacion.precio = 100; await db.SaveChangesAsync();
    await NoDisponible("El producto no tiene presentaciones con precio válido mayor al costo promedio.");
    presentacion.precio = 120; presentacion.unidades_equivalentes = 2; producto.Stock = 1; producto.PrecioPromedioCompra = 50;
    await db.SaveChangesAsync();
    await NoDisponible("Existencias insuficientes para las presentaciones disponibles.");
    producto.Stock = 100; producto.PrecioPromedioCompra = 100; presentacion.unidades_equivalentes = 1;
    var inactiva = new ProductoPresentacion { id_producto_presentacion = 999, id_producto = 1, IdPresentacion = 1,
        unidades_equivalentes = 2, precio = 250, estado = false };
    db.producto_presentaciones.Add(inactiva); await db.SaveChangesAsync();
    var filtrado = await buscador.BuscarPorCodigoBarra("001234");
    Check(filtrado?.presentaciones.Count == 1 && filtrado.presentaciones[0].id_producto_presentacion == 1,
        "Búsqueda devuelve únicamente presentaciones vendibles");
    db.producto_presentaciones.Remove(inactiva); await db.SaveChangesAsync();
}
CrearVentaDto Dto(decimal pago = 0, decimal descuento = 19) => new() { id_operacion = Guid.NewGuid(), id_cliente = 1, origen = "MOSTRADOR",
    detalles = [new() { id_producto = 1, id_producto_presentacion = 1, cantidad = 1, descuento = descuento }],
    pago = new() { monto = pago, metodo_pago = "EFECTIVO" } };
async Task<API_SISTEMA.Models.Ventas> Sell(CrearVentaDto dto)
{
    await using var db = new CheckDb(options);
    return await new CrearVentaService(db, new MovimientoCajaService(db), new PermisoUsuarioService(db)).CrearVenta(dto, 1);
}
async Task Rechazar(CrearVentaDto dto, string mensaje)
{
    try { await Sell(dto); } catch (ValidationException) { Check(true, mensaje); return; }
    throw new Exception("No rechazado: " + mensaje);
}
var original = Dto(50);
var venta = await Sell(original);
Check(venta.total == 101m && venta.monto_pagado == 50m && venta.saldo_pendiente == 51m, "Venta parcial y cliente inactivo permitidos");
var repetida = await Sell(original);
Check(repetida.id_ventas == venta.id_ventas, "Reintento devuelve la misma venta");
await using (var db = new CheckDb(options)) {
    Check(await db.ventas.CountAsync() == 1 && await db.pagos.CountAsync() == 1 && await db.movimientocaja.CountAsync() == 1 && (await db.productos.SingleAsync()).Stock == 99, "Sin duplicar inventario, cobro ni movimiento");
    var precio = await db.producto_presentaciones.SingleAsync(); precio.precio = 130; await db.SaveChangesAsync();
    Check((await new DetalleVentaService(db).ListarDetalleVenta(venta.id_ventas)).Single().precio == 120m, "El detalle conserva el precio original");
    var abono = new AbonarSaldoVentaService(db, new MovimientoCajaService(db), Auditoria(db));
    await abono.AbonarVenta(new() { id_venta = venta.id_ventas, monto = 51, observacion = "Prueba" }, 1);
    var pagada = await db.ventas.SingleAsync();
    Check(pagada.monto_pagado == 101 && pagada.saldo_pendiente == 0 && pagada.id_estado_venta == 1, "Abono actualiza monto pagado, saldo y estado");
}
original.detalles[0].descuento = 18;
try { await Sell(original); throw new Exception("Clave reutilizada aceptada"); } catch (VentaConflictoException) { Check(true, "Misma clave con otros datos rechazada"); }
var negativo = Dto(-1); await Rechazar(negativo, "Pago negativo rechazado");
var tarjeta = Dto(1); tarjeta.pago!.metodo_pago = "TARJETA"; await Rechazar(tarjeta, "Método distinto a efectivo rechazado");
await Rechazar(Dto(1000), "Sobrepago rechazado");
await Rechazar(Dto(0, 0.001m), "Descuento con tres decimales rechazado");
await using (var db = new CheckDb(options)) { var marca = await db.Marcas.SingleAsync(); marca.Estado = false; await db.SaveChangesAsync(); }
await Rechazar(Dto(), "Marca inactiva bloqueada");
await using (var db = new CheckDb(options)) { var marca = await db.Marcas.SingleAsync(); marca.Estado = true; var categoria = await db.categorias.SingleAsync(); categoria.Estado = false; await db.SaveChangesAsync(); }
await Rechazar(Dto(), "Categoría inactiva bloqueada");
await using (var db = new CheckDb(options)) {
    Check(await db.ventas.CountAsync() == 1 && (await db.productos.SingleAsync()).Stock == 99, "Rechazos revierten venta e inventario");
}
// Un fallo al insertar el movimiento revierte también la venta, su clave y el pago.
await using (var db = new CheckDb(options)) {
    var cat = await db.categorias.SingleAsync(); cat.Estado = true; await db.SaveChangesAsync();
    await db.Database.ExecuteSqlRawAsync("CREATE TRIGGER rechazo_movimiento BEFORE INSERT ON movimiento_caja BEGIN SELECT RAISE(ABORT, 'fallo de prueba'); END;");
}
try { await Sell(Dto(10)); throw new Exception("Movimiento fallido aceptado"); }
catch (DbUpdateException) { Check(true, "Fallo de movimiento revierte la transacción"); }
await using (var db = new CheckDb(options)) {
    Check(await db.ventas.CountAsync() == 1 && await db.VentaOperaciones.CountAsync() == 1 && await db.pagos.CountAsync() == 2 && (await db.productos.SingleAsync()).Stock == 99,
        "Rollback conserva inventario y no deja venta/pago/clave parciales");
    await db.Database.ExecuteSqlRawAsync("DROP TRIGGER rechazo_movimiento;");
}
var credito = await Sell(Dto());
Check(credito.saldo_pendiente == credito.total && credito.monto_pagado == 0, "Crédito total permitido sin pago inicial");
async Task Abonar(int idVenta, decimal monto)
{
    await using var db = new CheckDb(options);
    await new AbonarSaldoVentaService(db, new MovimientoCajaService(db), Auditoria(db)).AbonarVenta(
        new() { id_venta = idVenta, monto = monto, observacion = "Prueba abonos" }, 1);
}
foreach (var monto in new[] { 0m, -1m, 0.001m, credito.total + 1 })
{
    try { await Abonar(credito.id_ventas, monto); throw new Exception("Abono inválido aceptado"); }
    catch (ValidationException) { Check(true, "Abono inválido rechazado: " + monto); }
}
try { await Abonar(venta.id_ventas, 1); throw new Exception("Abono a venta pagada aceptado"); }
catch (ValidationException) { Check(true, "Venta liquidada rechaza nuevos abonos"); }
try { await Abonar(int.MaxValue, 1); throw new Exception("Venta inexistente aceptada"); }
catch (KeyNotFoundException) { Check(true, "Abono rechaza venta inexistente"); }
await using (var db = new CheckDb(options))
    await db.Database.ExecuteSqlRawAsync("CREATE TRIGGER rechazo_abono BEFORE INSERT ON movimiento_caja BEGIN SELECT RAISE(ABORT, 'fallo de prueba'); END;");
try { await Abonar(credito.id_ventas, 10); throw new Exception("Abono fallido aceptado"); }
catch (DbUpdateException) { Check(true, "Fallo de movimiento rechaza abono"); }
await using (var db = new CheckDb(options))
{
    var guardada = await db.ventas.SingleAsync(v => v.id_ventas == credito.id_ventas);
    Check(!await db.pagos.AnyAsync(p => p.id_venta == credito.id_ventas) && guardada.monto_pagado == 0 && guardada.saldo_pendiente == credito.total,
        "Fallo revierte pago y saldo del abono");
    await db.Database.ExecuteSqlRawAsync("DROP TRIGGER rechazo_abono;");
}
await Abonar(credito.id_ventas, 10);
await using (var db = new CheckDb(options))
{
    var guardada = await db.ventas.SingleAsync(v => v.id_ventas == credito.id_ventas);
    var pago = await db.pagos.SingleAsync(p => p.id_venta == credito.id_ventas);
    Check(guardada.monto_pagado == 10 && guardada.saldo_pendiente == credito.total - 10 && guardada.id_estado_venta == 2,
        "Abono parcial mantiene venta pendiente");
    Check(await db.movimientocaja.AnyAsync(m => m.id_pago_venta == pago.id_pago && m.id_venta == credito.id_ventas &&
        m.id_tipo_movimiento == TiposMovimientoCaja.AbonoVenta && m.monto == 10 && m.id_usuario == 1), "Abono registra movimiento vinculado y responsable");
    var evento = await db.AuditoriaEventos.Include(e => e.Detalles).SingleAsync(e => e.Entidad == "pagos" && e.IdRegistro == pago.id_pago.ToString());
    Check(evento.Accion == "ABONO_VENTA_REGISTRADO" && evento.IdUsuario == 1 && evento.UsuarioResponsable == "prueba" &&
        evento.Resultado == "EXITOSO" && !string.IsNullOrWhiteSpace(evento.TraceId), "Auditoria de abono identifica pago y responsable");
    Check(evento.Detalles.Any(d => d.Campo == "id_venta" && d.ValorNuevo == credito.id_ventas.ToString()) &&
        evento.Detalles.Any(d => d.Campo == "saldo_anterior") && evento.Detalles.Any(d => d.Campo == "saldo_nuevo") &&
        evento.Detalles.Any(d => d.Campo == "monto" && d.ValorNuevo == "10"), "Auditoria conserva monto, venta y cambio de saldo");
}
await using (var db = new CheckDb(options))
{
    var tabla = db.Model.FindEntityType(typeof(AuditoriaEvento))!.GetTableName();
    await db.Database.ExecuteSqlRawAsync($"CREATE TRIGGER rechazo_auditoria_abono BEFORE INSERT ON [{tabla}] BEGIN SELECT RAISE(ABORT, 'fallo auditoria'); END;");
}
try { await Abonar(credito.id_ventas, 1); throw new Exception("Abono sin auditoria aceptado"); }
catch (DbUpdateException) { Check(true, "Fallo de auditoria rechaza abono"); }
await using (var db = new CheckDb(options))
{
    var guardada = await db.ventas.SingleAsync(v => v.id_ventas == credito.id_ventas);
    Check(guardada.monto_pagado == 10 && await db.pagos.CountAsync(p => p.id_venta == credito.id_ventas) == 1 &&
        await db.movimientocaja.CountAsync(m => m.id_venta == credito.id_ventas) == 1 &&
        await db.AuditoriaEventos.CountAsync(e => e.Accion == "ABONO_VENTA_REGISTRADO") == 2,
        "Fallo de auditoria revierte pago, saldo y movimiento sin evento exitoso");
    await db.Database.ExecuteSqlRawAsync("DROP TRIGGER rechazo_auditoria_abono;");
}
await Abonar(credito.id_ventas, credito.total - 10);
await using (var db = new CheckDb(options))
{
    var guardada = await db.ventas.SingleAsync(v => v.id_ventas == credito.id_ventas);
    Check(guardada.saldo_pendiente == 0 && guardada.monto_pagado == credito.total && guardada.id_estado_venta == 1,
        "Abonos sucesivos liquidan la venta");
}
await using (var db = new CheckDb(options)) {
    var before = await db.ventas.CountAsync();
    var creation = new CrearVentaService(db, new MovimientoCajaService(db), new PermisoUsuarioService(db));
    using var cancel = new CancellationTokenSource(); cancel.Cancel();
    try { await creation.CrearVenta(Dto(), 1, cancel.Token); throw new Exception("Cancelación ignorada"); }
    catch (OperationCanceledException) { Check(await db.ventas.CountAsync() == before, "Cancelación no escribe venta"); }
}
var action = new Microsoft.AspNetCore.Mvc.ActionContext(new Microsoft.AspNetCore.Http.DefaultHttpContext(), new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
foreach (var filtroCaja in new Microsoft.AspNetCore.Mvc.Filters.IExceptionFilter[]
{
    new VentaExceptionFilter(Microsoft.Extensions.Logging.Abstractions.NullLogger<VentaExceptionFilter>.Instance),
    new PedidoCatalogoExceptionFilter(Microsoft.Extensions.Logging.Abstractions.NullLogger<PedidoCatalogoExceptionFilter>.Instance)
})
{
    var errorCaja = new Microsoft.AspNetCore.Mvc.Filters.ExceptionContext(action, [])
    { Exception = new CajaValidationException("Abre la caja para poder registrar o aprobar la venta.", "CAJA_NO_ABIERTA") };
    filtroCaja.OnException(errorCaja);
    var respuestaCaja = (Microsoft.AspNetCore.Mvc.ObjectResult)errorCaja.Result!;
    Check(respuestaCaja.StatusCode == 409 && JsonSerializer.SerializeToElement(respuestaCaja.Value)
        .GetProperty("codigo").GetString() == "CAJA_NO_ABIERTA", "Ventas y pedidos exponen código de caja cerrada");
}
var errorContext = new Microsoft.AspNetCore.Mvc.Filters.ExceptionContext(action, []) { Exception = new Exception("SQL password=secret stack") };
new VentaExceptionFilter(Microsoft.Extensions.Logging.Abstractions.NullLogger<VentaExceptionFilter>.Instance).OnException(errorContext);
var result = (Microsoft.AspNetCore.Mvc.ObjectResult)errorContext.Result!;
Check(result.StatusCode == 500 && !JsonSerializer.Serialize(result.Value).Contains("secret"), "Errores internos sin filtración");
// Regresión: crear y editar más de 100 detalles sin un máximo de líneas de negocio.
await using (var db = new CheckDb(options)) {
    for (int i = 2; i <= 102; i++) {
        db.productos.Add(new Productos { IdProducto = i, IdMarca = 1, Nombre = $"Producto {i}", Stock = 2, PrecioPromedioCompra = 100m });
        db.producto_presentaciones.Add(new ProductoPresentacion { id_producto_presentacion = i, id_producto = i, IdPresentacion = 1, estado = true, unidades_equivalentes = 1, precio = 120m });
    }
    await db.SaveChangesAsync();
}
var extensa = new CrearVentaDto { id_operacion = Guid.NewGuid(), id_cliente = 1, origen = "MOSTRADOR",
    detalles = Enumerable.Range(2, 101).Select(i => new CrearDetalleVentaDto { id_producto = i, id_producto_presentacion = i, cantidad = 1 }).ToList() };
var ventaExtensa = await Sell(extensa);
await using (var db = new CheckDb(options)) {
    Check(await db.detalle_Ventas.CountAsync(d => d.id_venta == ventaExtensa.id_ventas) == 101 && ventaExtensa.total == 12120m,
        "Creación acepta 101 detalles y calcula el total completo");
    await new ActualizarVentaService(db).ModificarVenta(ventaExtensa.id_ventas,
        new ModificarVentaDto { id_cliente = 1, detalles = Enumerable.Range(2, 101).Select(i => new ActualizarDetalleVentaDto {
            id_producto = i, id_producto_presentacion = i, cantidad = 2 }).ToList() }, 1);
    Check(await db.detalle_Ventas.CountAsync(d => d.id_venta == ventaExtensa.id_ventas) == 101 &&
        (await db.ventas.SingleAsync(v => v.id_ventas == ventaExtensa.id_ventas)).total == 24240m,
        "Edición acepta 101 detalles y conserva los cálculos de inventario y total");
}
var vacia = Dto(); vacia.detalles = [];
await Rechazar(vacia, "Sin límite máximo, la venta vacía sigue rechazada");
// Clientes: el índice es mínimo; el detalle se recupera solo por su ID.
await using (var db = new CheckDb(options)) {
    var cliente = await db.cliente.SingleAsync(c => c.id_cliente == 1);
    cliente.apellido = "Apellido"; cliente.nit = "12345"; cliente.dpi = "98765"; cliente.telefono = "5555";
    cliente.correo_electronico = "cliente@example.test"; cliente.direccion = "Ciudad";
    await db.SaveChangesAsync(); db.ChangeTracker.Clear();
    var servicioClientes = new ClienteService(db);
    var indice = await servicioClientes.ListarClientes();
    Check(indice.Single(c => c.id_cliente == 1).nombre == "Cliente inactivo permitido Apellido", "Índice incluye nombre completo y clientes inactivos");
    var jsonIndice = JsonSerializer.SerializeToElement(indice);
    Check(jsonIndice[0].EnumerateObject().Select(p => p.Name).Order().SequenceEqual(new[] { "id_cliente", "nombre" }), "Listado devuelve únicamente ID y nombre");
    Check(!db.ChangeTracker.Entries<Cliente>().Any(), "Consultas de clientes no mantienen tracking");
    var ficha = await servicioClientes.ObtenerCliente(1);
    Check(ficha is not null && ficha.nombre == cliente.nombre && ficha.nit == "12345" && ficha.direccion == "Ciudad", "Detalle recupera los campos del cliente seleccionado");
    Check(JsonSerializer.SerializeToElement(ficha).GetProperty("id_cliente").GetInt32() == 1, "Detalle usa el mismo identificador JSON que el índice");
    var controladorClientes = new API_SISTEMA.Controllers.ClienteController(servicioClientes);
    Check(await controladorClientes.Obtener(int.MaxValue) is Microsoft.AspNetCore.Mvc.NotFoundObjectResult, "Cliente inexistente devuelve 404");
    using var cancelCliente = new CancellationTokenSource(); cancelCliente.Cancel();
    try { await servicioClientes.ListarClientes(cancelCliente.Token); throw new Exception("Cancelación de clientes ignorada"); }
    catch (OperationCanceledException) { Check(true, "Listado de clientes respeta cancelación"); }
}
var autorizacionCliente = typeof(API_SISTEMA.Controllers.ClienteController).GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true)
    .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Single();
Check(autorizacionCliente.Roles == Roles.Administrador + "," + Roles.Vendedor, "Consultas de clientes restringidas a los roles internos");
var errorCliente = new Microsoft.AspNetCore.Mvc.Filters.ExceptionContext(action, []) { Exception = new Exception("SQL password=secret internal stack") };
new ClienteExceptionFilter(Microsoft.Extensions.Logging.Abstractions.NullLogger<ClienteExceptionFilter>.Instance).OnException(errorCliente);
var resultadoCliente = (Microsoft.AspNetCore.Mvc.ObjectResult)errorCliente.Result!;
Check(resultadoCliente.StatusCode == 500 && !JsonSerializer.Serialize(resultadoCliente.Value).Contains("secret"), "Clientes no expone errores internos");
// El listado se limita al turno abierto, sin mezclar sesiones del mismo día.
await using (var db = new CheckDb(options))
{
    var listado = new VentaService(db);
    var esperadas = await db.ventas.Where(v => v.id_sesion_caja == 1).Select(v => v.id_ventas).ToListAsync();
    var inicial = await listado.ListarVentas(1);
    Check(inicial.Count > 0 && inicial.All(v => v.ganancia_total is null), "Vendedor no recibe ganancia en el listado");
    Check(!JsonSerializer.Serialize(inicial).Contains("ganancia_total"), "JSON del vendedor omite el campo de ganancia");
    db.rols.Add(new Rol { id_rol = 2, nombre = Roles.Administrador, descripcion = "Admin", estado = true });
    db.usuarios.Add(new Usuario { id_usuario = 2, id_rol = 2, nombre = "Admin", apellido = "Prueba", usuario = "admin", telefono = "456", estado = true });
    await db.SaveChangesAsync();
    var vistaAdmin = await listado.ListarVentas(2);
    var ganancias = await db.ventas.Where(v => v.id_sesion_caja == 1).ToDictionaryAsync(v => v.id_ventas, v => v.ganancia_total);
    Check(vistaAdmin.Count > 0 && vistaAdmin.All(v => v.ganancia_total == ganancias[v.id_ventas]), "Administrador recibe la ganancia guardada de cada venta");
    Check(JsonSerializer.Serialize(vistaAdmin).Contains("ganancia_total"), "JSON del administrador incluye ganancia");
    Check(inicial.Select(v => v.id_ventas).Order().SequenceEqual(esperadas.Order()), "Listado incluye todas las ventas de caja abierta");
    db.sesioncaja.Add(new SesionCaja { id_sesion_caja = 2, id_caja = 1, id_usuario_apertura = 1,
        fecha_apertura = DateTime.Now.AddHours(-2), fecha_cierre = DateTime.Now.AddHours(-1),
        monto_inicial = 0, observacion_apertura = "Turno cerrado del mismo día" });
    var historica = new API_SISTEMA.Models.Ventas { id_sesion_caja = 2, id_cliente = 1, id_usuario = 1,
        id_estado_venta = 2, total = 10, subtotal = 10, saldo_pendiente = 10, origen = "PRUEBA", fecha_venta = DateTime.Now };
    db.ventas.Add(historica);
    await db.SaveChangesAsync();
    var actual = await listado.ListarVentas(1);
    Check(actual.All(v => v.id_ventas != historica.id_ventas) && actual.Count == inicial.Count,
        "Listado excluye ventas de caja cerrada del mismo día");
    var primera = await listado.ListarVentas(1, new() { pagina = 1, tamanoPagina = 1 });
    var segunda = await listado.ListarVentas(1, new() { pagina = 2, tamanoPagina = 1 });
    Check(primera.Count == 1 && segunda.Count == 1 && primera[0].id_ventas != segunda[0].id_ventas,
        "Listado pagina sin repetir ventas");
    var caja = await db.sesioncaja.SingleAsync(c => c.id_sesion_caja == 1);
    var otraVenta = new API_SISTEMA.Models.Ventas { id_sesion_caja = 1, id_cliente = 1, id_usuario = 2,
        id_estado_venta = 2, total = 25, subtotal = 25, saldo_pendiente = 25, origen = "PRUEBA", fecha_venta = DateTime.Now };
    db.ventas.Add(otraVenta); await db.SaveChangesAsync();
    var filtroUsuario = new FiltrarVentasUsuarioService(db, listado);
    var delVendedor = await filtroUsuario.Listar(2, 1);
    Check(delVendedor.Count > 0 && delVendedor.All(v => v.id_usuario == 1 && v.id_ventas != historica.id_ventas) &&
        delVendedor.All(v => v.ganancia_total.HasValue), "Filtro usuario: admin consulta solo responsable y caja activa con ganancias");
    var delAdmin = await filtroUsuario.Listar(2, 2);
    Check(delAdmin.Count == 1 && delAdmin[0].id_ventas == otraVenta.id_ventas, "Filtro usuario: excluye ventas de otros responsables");
    var propias = await filtroUsuario.Listar(1, 1);
    Check(propias.Count == delVendedor.Count && propias.All(v => v.ganancia_total is null), "Filtro usuario: vendedor consulta propias sin ganancias");
    try { await filtroUsuario.Listar(1, 2); throw new Exception("Vendedor consultó ventas ajenas"); }
    catch (UnauthorizedAccessException) { Check(true, "Filtro usuario: bloquea vendedor consultando otro usuario"); }
    try { await filtroUsuario.Listar(2, int.MaxValue); throw new Exception("Usuario inexistente aceptado"); }
    catch (KeyNotFoundException) { Check(true, "Filtro usuario: usuario inexistente devuelve no encontrado"); }
    try { await filtroUsuario.Listar(2, 0); throw new Exception("ID inválido aceptado"); }
    catch (ValidationException) { Check(true, "Filtro usuario: valida identificador"); }
    var paginaUsuario1 = await filtroUsuario.Listar(2, 1, new() { pagina = 1, tamanoPagina = 1 });
    var paginaUsuario2 = await filtroUsuario.Listar(2, 1, new() { pagina = 2, tamanoPagina = 1 });
    Check(paginaUsuario1.Count == 1 && paginaUsuario2.Count == 1 && paginaUsuario1[0].id_ventas != paginaUsuario2[0].id_ventas,
        "Filtro usuario: pagina después de filtrar");
    Check((await filtroUsuario.Listar(2, 1, new() { desde = DateTime.Now.AddYears(1) })).Count == 0,
        "Filtro usuario: conserva filtros de fecha");
    var servicioResumen = new ResumenVentasService(db, listado, new HistorialVentasFechaService(db, listado));
    var esperadoGanancia = await db.ventas.Where(v => v.id_sesion_caja == 1).SumAsync(v => v.ganancia_total);
    var resumenPagina1 = await servicioResumen.Consultar(2, new() { TamanoPagina = 1 });
    var resumenPagina2 = await servicioResumen.Consultar(2, new() { TamanoPagina = 1, Pagina = 2 });
    Check(resumenPagina1.Items.Count == 1 && resumenPagina1.GananciaTotal == esperadoGanancia && resumenPagina1.HayMas &&
        resumenPagina2.GananciaTotal == esperadoGanancia, "Resumen: suma ganancias de toda la caja antes de paginar");
    var resumenUsuario = await servicioResumen.Consultar(2, new() { IdUsuario = 1, TamanoPagina = 1 });
    Check(resumenUsuario.GananciaTotal == await db.ventas.Where(v => v.id_sesion_caja == 1 && v.id_usuario == 1).SumAsync(v => v.ganancia_total),
        "Resumen: ganancia respeta usuario en caja activa");
    try { await servicioResumen.Consultar(1, new()); throw new Exception("Vendedor recibió ganancias"); }
    catch (UnauthorizedAccessException) { Check(true, "Resumen: solo administrador puede consultar ganancias"); }
    var resumenVendedor = new VentasResumenPaginaDto { Items = propias };
    Check(!JsonSerializer.Serialize(resumenVendedor).Contains("GananciaTotal") && resumenVendedor.Items.All(v => v.ganancia_total is null),
        "Resumen: JSON del vendedor omite ganancia agregada e individual");
    caja.fecha_cierre = DateTime.Now;
    await db.SaveChangesAsync();
    Check((await listado.ListarVentas(1)).Count == 0, "Sin caja abierta el listado queda vacío");
    Check((await filtroUsuario.Listar(2, 1)).Count == 0, "Filtro usuario: sin caja abierta devuelve lista vacía");
    Check((await servicioResumen.Consultar(2, new())).GananciaTotal == 0, "Resumen: caja cerrada devuelve ganancia cero");
    var historialFechas = new HistorialVentasFechaService(db, listado);
    var inicioDia = new DateTime(2020, 5, 10);
    API_SISTEMA.Models.Ventas VentaFecha(DateTime fecha, int sesion) => new()
    { id_sesion_caja = sesion, id_cliente = 1, id_usuario = 1, id_estado_venta = 2,
        total = 25, subtotal = 25, saldo_pendiente = 25, origen = "PRUEBA_FECHAS", fecha_venta = fecha };
    var antesDia = VentaFecha(inicioDia.AddMilliseconds(-1), 1);
    var inicio = VentaFecha(inicioDia, 1);
    var finalDia = VentaFecha(inicioDia.AddDays(1).AddMilliseconds(-1), 2);
    var siguienteDia = VentaFecha(inicioDia.AddDays(1), 2);
    db.ventas.AddRange(antesDia, inicio, finalDia, siguienteDia); await db.SaveChangesAsync();
    var filtroDia = new HistorialVentasFechaDto { Desde = new(2020, 5, 10), Hasta = new(2020, 5, 10) };
    var delDia = await historialFechas.Listar(2, filtroDia);
    Check(delDia.Select(v => v.id_ventas).Order().SequenceEqual(new[] { inicio.id_ventas, finalDia.id_ventas }.Order()),
        "Historial: incluye desde medianoche y excluye medianoche siguiente en dos sesiones cerradas");
    var pagDia1 = await historialFechas.Listar(2, new() { Desde = filtroDia.Desde, Hasta = filtroDia.Hasta, TamanoPagina = 1 });
    var pagDia2 = await historialFechas.Listar(2, new() { Desde = filtroDia.Desde, Hasta = filtroDia.Hasta, TamanoPagina = 1, Pagina = 2 });
    Check(pagDia1[0].id_ventas == finalDia.id_ventas && pagDia2[0].id_ventas == inicio.id_ventas,
        "Historial: orden y paginación dentro del rango");
    var fechaOtroUsuario = VentaFecha(inicioDia.AddHours(12), 2);
    fechaOtroUsuario.id_usuario = 2;
    db.ventas.Add(fechaOtroUsuario); await db.SaveChangesAsync();
    var fechaUsuario = await historialFechas.Listar(2, new() { Desde = filtroDia.Desde, Hasta = filtroDia.Hasta, IdUsuario = 2 });
    Check(fechaUsuario.Count == 1 && fechaUsuario[0].id_ventas == fechaOtroUsuario.id_ventas,
        "Historial: usuario y fecha filtran todas las sesiones del día");
    var fechaVendedor = await historialFechas.Listar(2, new() { Desde = filtroDia.Desde, Hasta = filtroDia.Hasta, IdUsuario = 1 });
    Check(fechaVendedor.Count == 2 && fechaVendedor.All(v => v.id_usuario == 1),
        "Historial: el usuario no incluye sus ventas de otros días");
    Check((await historialFechas.Listar(2, filtroDia)).Count == 3, "Historial: sin usuario seleccionado muestra todos los del día");
    inicio.ganancia_total = 4; finalDia.ganancia_total = 6; fechaOtroUsuario.ganancia_total = 8;
    await db.SaveChangesAsync();
    var resumenDia = await servicioResumen.Consultar(2, new() { Desde = filtroDia.Desde, Hasta = filtroDia.Hasta, TamanoPagina = 1 });
    var resumenDiaUsuario = await servicioResumen.Consultar(2, new() { Desde = filtroDia.Desde, Hasta = filtroDia.Hasta, IdUsuario = 1, TamanoPagina = 1 });
    Check(resumenDia.GananciaTotal == 18 && resumenDia.TotalRegistros == 3 && resumenDia.Items.Count == 1 && resumenDiaUsuario.GananciaTotal == 10,
        "Resumen: suma por día y usuario entre sesiones cerradas sin limitarse a la página");
    try { await historialFechas.Listar(2, new() { IdUsuario = int.MaxValue }); throw new Exception("Usuario desconocido aceptado"); }
    catch (KeyNotFoundException) { Check(true, "Historial: valida usuario del filtro combinado"); }
    var hoyGt = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "America/Guatemala"));
    var inicioSemana = hoyGt.AddDays(-6).ToDateTime(TimeOnly.MinValue);
    var semanaIncluida = VentaFecha(inicioSemana, 2);
    var semanaExcluida = VentaFecha(inicioSemana.AddMilliseconds(-1), 1);
    db.ventas.AddRange(semanaIncluida, semanaExcluida); await db.SaveChangesAsync();
    var semana = await historialFechas.Listar(2, new());
    Check(semana.Any(v => v.id_ventas == semanaIncluida.id_ventas) && semana.All(v => v.id_ventas != semanaExcluida.id_ventas),
        "Historial: últimos siete días de Guatemala incluyendo hoy");
    try { await historialFechas.Listar(1, filtroDia); throw new Exception("Vendedor accedió al historial"); }
    catch (UnauthorizedAccessException) { Check(true, "Historial: acceso exclusivo para administrador"); }
    try { await historialFechas.Listar(2, new() { Desde = new(2020, 5, 11), Hasta = new(2020, 5, 10) }); throw new Exception("Rango invertido aceptado"); }
    catch (ValidationException) { Check(true, "Historial: rechaza rango invertido"); }
}
await using (var db = new CheckDb(options))
{
    var inventario = new API_SISTEMA.Services.Productos.ProductoInventarioService(db);
    var productos = await db.productos.AsNoTracking().ToListAsync();
    var esperado = productos.Where(p => p.PrecioPromedioCompra > 0).Sum(p => (p.Stock ?? 0) * p.PrecioPromedioCompra!.Value);
    var primera = await inventario.Listar(new() { Pagina = 1, TamanoPagina = 1 });
    var segunda = await inventario.Listar(new() { Pagina = 2, TamanoPagina = 1 });
    Check(primera.Items.Count == 1 && primera.Total == productos.Count && primera.ValorInventarioTotal == esperado,
        "Inventario: suma todos los productos antes de paginar");
    Check(segunda.ValorInventarioTotal == primera.ValorInventarioTotal, "Inventario: total independiente de página");
    var filtrado = await inventario.Listar(new() { Texto = "Prueba", TamanoPagina = 1 });
    Check(filtrado.ValorInventarioTotal == productos.Where(p => p.Nombre.Contains("Prueba") && p.PrecioPromedioCompra > 0)
        .Sum(p => (p.Stock ?? 0) * p.PrecioPromedioCompra!.Value), "Inventario: suma respeta filtros");
    var vacio = await inventario.Listar(new() { Texto = "NO_EXISTE_INVENTARIO" });
    Check(vacio.Total == 0 && vacio.ValorInventarioTotal == 0 && vacio.ProductosSinCosto == 0,
        "Inventario: consulta vacía devuelve cero");
    var producto = productos.First(p => p.IdProducto == 2);
    await db.productos.Where(p => p.IdProducto == 2).ExecuteUpdateAsync(s => s.SetProperty(p => p.PrecioPromedioCompra, (decimal?)null));
    var sinCosto = await inventario.Listar(new() { TamanoPagina = 1 });
    Check(sinCosto.ValorInventarioTotal == esperado - (producto.Stock ?? 0) * producto.PrecioPromedioCompra!.Value &&
        sinCosto.ProductosSinCosto == productos.Count(p => !p.PrecioPromedioCompra.HasValue || p.PrecioPromedioCompra <= 0) + 1,
        "Inventario: excluye costos desconocidos e informa su cantidad");
    await db.productos.Where(p => p.IdProducto == 2).ExecuteUpdateAsync(s => s.SetProperty(p => p.PrecioPromedioCompra, producto.PrecioPromedioCompra));
}
await PedidosCatalogoChecks.Ejecutar(options, Check);
await using (var db = new CheckDb(options))
{
    var servicioPagos = new PagoService(db);
    var idsOriginales = await db.pagos.Where(p => p.id_venta == venta.id_ventas).OrderBy(p => p.id_pago)
        .Select(p => p.id_pago).ToListAsync();
    await db.pagos.Where(p => p.id_pago == idsOriginales[0])
        .ExecuteUpdateAsync(s => s.SetProperty(p => p.id_sesion_caja, 2));
    await db.sesioncaja.Where(c => c.fecha_cierre == null)
        .ExecuteUpdateAsync(s => s.SetProperty(c => c.fecha_cierre, (DateTime?)DateTime.Now));
    var historialPagos = await servicioPagos.ListarPorVenta(venta.id_ventas);
    Check(historialPagos.Count == 2 && historialPagos.Sum(p => p.Monto) == 101 &&
        historialPagos.Select(p => p.IdSesionCaja).Distinct().Count() == 2,
        "Historial de pagos: incluye pago inicial y abono de dos sesiones sin caja abierta");
    Check(historialPagos.All(p => p.IdVenta == venta.id_ventas && p.IdUsuario == 1 && p.Usuario == "Usuario Prueba" && p.MetodoPago == "EFECTIVO"),
        "Historial de pagos: solo la venta solicitada, con responsable y método");
    Check(historialPagos.Select(p => p.IdPago).Order().SequenceEqual(idsOriginales.Order()), "Historial de pagos: no omite ni duplica pagos");
    Check(historialPagos.Zip(historialPagos.Skip(1)).All(p => p.First.FechaPago > p.Second.FechaPago ||
        (p.First.FechaPago == p.Second.FechaPago && p.First.IdPago > p.Second.IdPago)), "Historial de pagos: orden estable más recientes primero");
    Check((await servicioPagos.ListarPorVenta(ventaExtensa.id_ventas)).Count == 0, "Historial de pagos: venta sin pagos devuelve lista vacía");
    try { await servicioPagos.ListarPorVenta(int.MaxValue); throw new Exception("Venta inexistente aceptada"); }
    catch (KeyNotFoundException) { Check(true, "Historial de pagos: venta inexistente devuelve no encontrado"); }
    try { await servicioPagos.ListarPorVenta(0); throw new Exception("ID inválido aceptado"); }
    catch (ValidationException) { Check(true, "Historial de pagos: valida identificador"); }
    using var cancelPagos = new CancellationTokenSource(); cancelPagos.Cancel();
    try { await servicioPagos.ListarPorVenta(venta.id_ventas, cancelPagos.Token); throw new Exception("Cancelación ignorada"); }
    catch (OperationCanceledException) { Check(true, "Historial de pagos: respeta cancelación"); }
}
Console.WriteLine($"{passed} comprobaciones aprobadas; base SQLite temporal, sin conexión a la base del negocio.");

sealed class CheckDb(DbContextOptions<SistemaDbContext> options) : SistemaDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        var keep = new HashSet<Type> { typeof(ZonaEntrega), typeof(PedidoCatalogo), typeof(DetallePedidoCatalogo), typeof(Usuario), typeof(Rol), typeof(RolPermiso), typeof(TablaPermiso), typeof(UsuarioPermiso),
            typeof(Productos), typeof(Marca), typeof(Categoria), typeof(ProductoPresentacion), typeof(Presentacion), typeof(Cliente),
            typeof(API_SISTEMA.Models.Ventas), typeof(DetalleVenta), typeof(EstadoVenta), typeof(Pagos), typeof(SesionCaja), typeof(Caja), typeof(MovimientoCaja), typeof(TipoMovimientoCaja), typeof(VentaOperacion), typeof(AuditoriaEvento), typeof(AuditoriaEventoDetalle) };
        foreach (var e in b.Model.GetEntityTypes().ToArray()) if (!keep.Contains(e.ClrType)) b.Ignore(e.ClrType);
        var detalle = b.Entity<AuditoriaEventoDetalle>();
        foreach (var constraint in detalle.Metadata.GetCheckConstraints().ToArray())
            detalle.Metadata.RemoveCheckConstraint(constraint.Name);
        detalle.Property(d => d.ValorAnterior).HasColumnType("TEXT");
        detalle.Property(d => d.ValorNuevo).HasColumnType("TEXT");
        var auditoria = b.Entity<AuditoriaEvento>();
        foreach (var constraint in auditoria.Metadata.GetCheckConstraints().ToArray())
            auditoria.Metadata.RemoveCheckConstraint(constraint.Name);
        auditoria.Property(e => e.FechaUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
        // Adaptación de expresiones específicas de SQL Server para la base temporal.
        foreach (var type in new[] { typeof(ZonaEntrega), typeof(PedidoCatalogo), typeof(DetallePedidoCatalogo) })
        {
            var entity = b.Entity(type);
            foreach (var constraint in entity.Metadata.GetCheckConstraints().ToArray())
                entity.Metadata.RemoveCheckConstraint(constraint.Name);
        }
        b.Entity<PedidoCatalogo>().Property(p => p.FechaUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
        b.Entity<DetallePedidoCatalogo>().Property(d => d.Subtotal).HasComputedColumnSql("cantidad * precio_unitario", stored: true);
    }
}
