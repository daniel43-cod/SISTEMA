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
    var abono = new AbonarSaldoVentaService(db, new MovimientoCajaService(db));
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
await using (var db = new CheckDb(options)) {
    var before = await db.ventas.CountAsync();
    var creation = new CrearVentaService(db, new MovimientoCajaService(db), new PermisoUsuarioService(db));
    using var cancel = new CancellationTokenSource(); cancel.Cancel();
    try { await creation.CrearVenta(Dto(), 1, cancel.Token); throw new Exception("Cancelación ignorada"); }
    catch (OperationCanceledException) { Check(await db.ventas.CountAsync() == before, "Cancelación no escribe venta"); }
}
var action = new Microsoft.AspNetCore.Mvc.ActionContext(new Microsoft.AspNetCore.Http.DefaultHttpContext(), new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
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
Console.WriteLine($"{passed} comprobaciones aprobadas; base SQLite temporal, sin conexión a la base del negocio.");

sealed class CheckDb(DbContextOptions<SistemaDbContext> options) : SistemaDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        var keep = new HashSet<Type> { typeof(Usuario), typeof(Rol), typeof(RolPermiso), typeof(TablaPermiso), typeof(UsuarioPermiso),
            typeof(Productos), typeof(Marca), typeof(Categoria), typeof(ProductoPresentacion), typeof(Presentacion), typeof(Cliente),
            typeof(API_SISTEMA.Models.Ventas), typeof(DetalleVenta), typeof(EstadoVenta), typeof(Pagos), typeof(SesionCaja), typeof(Caja), typeof(MovimientoCaja), typeof(TipoMovimientoCaja), typeof(VentaOperacion) };
        foreach (var e in b.Model.GetEntityTypes().ToArray()) if (!keep.Contains(e.ClrType)) b.Ignore(e.ClrType);
    }
}
