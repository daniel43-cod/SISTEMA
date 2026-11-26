using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Ventas;
using API_SISTEMA.Dtos.PedidosCatalogo;
using API_SISTEMA.Services.PedidosCatalogo;
using API_SISTEMA.Models;
using API_SISTEMA.Services;
using API_SISTEMA.Services.Ventas;
using API_SISTEMA.Services.MovimientoCaja;
using API_SISTEMA.Services.Permisos;
using API_SISTEMA.Utilidades;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

try { await Run(); } catch (Exception e) { Console.Error.WriteLine(e); Environment.ExitCode = 1; }
static async Task Run()
{
    int passed = 0;
    void Check(bool condition, string name) { if (!condition) throw new Exception(name); passed++; Console.WriteLine("OK " + name); }
    CrearVentaDto Request() => new() { id_operacion = Guid.NewGuid(), id_cliente = 1,
        detalles = [new() { id_surtido = 1, id_producto_presentacion = 1, cantidad = 1,
            componentes = [new() { id_producto = 1, cantidad = 3 }, new() { id_producto = 2, cantidad = 3 }] }] };
    async Task Case(string name, Action<CrearVentaDto> change, Func<CheckDb, Task>? arrange = null, Type? exception = null)
    {
        await using var f = await Fixture.Create();
        var dto = Request(); change(dto);
        if (arrange is not null) { await arrange(f.Db); f.Db.ChangeTracker.Clear(); }
        var before = await f.Db.productos.AsNoTracking().OrderBy(p => p.IdProducto).Select(p => p.Stock).ToArrayAsync();
        try { await f.Sell(dto); throw new Exception("Se aceptó " + name); }
        catch (Exception e) when ((exception ?? typeof(ValidationException)).IsAssignableFrom(e.GetType())) { }
        f.Db.ChangeTracker.Clear();
        var after = await f.Db.productos.AsNoTracking().OrderBy(p => p.IdProducto).Select(p => p.Stock).ToArrayAsync();
        Check(await f.Db.ventas.CountAsync() == 0 && await f.Db.DetalleVentaSurtidos.CountAsync() == 0 &&
            before.SequenceEqual(after), name + ": sin persistencia parcial");
    }
    await using (var f = await Fixture.Create())
    {
        var dto = Request(); dto.pago = new() { monto = 10, metodo_pago = "EFECTIVO" };
        var v = await f.Sell(dto);
        Check(v.total == 20 && v.ganancia_total == 4.91m && v.saldo_pendiente == 10, "Composición 3+3 calcula costo real y ganancia");
        Check((await f.Db.productos.FindAsync(1))!.Stock == 17 && (await f.Db.productos.FindAsync(2))!.Stock == 17, "Descuenta cada sabor");
        Check(await f.Db.DetalleVentaSurtidos.CountAsync() == 2 && await f.Db.DetalleVentaSurtidos.AnyAsync(c => c.IdProducto == 2 && c.CostoUnitarioVenta == 2.70m), "Guarda cantidades y costos históricos de componentes");
        var repeat = await f.Sell(dto);
        Check(repeat.id_ventas == v.id_ventas && await f.Db.pagos.CountAsync() == 1 && await f.Db.movimientocaja.CountAsync() == 1,
            "Idempotencia no duplica surtido, stock ni pago");
        dto.detalles[0].componentes![0].cantidad = 4;
        try { await f.Sell(dto); throw new Exception("Reutilizó clave modificada"); }
        catch (VentaConflictoException) { Check(true, "Misma clave con composición distinta devuelve conflicto"); }
        var detail = (await new DetalleVentaService(f.Db).ListarDetalleVenta(v.id_ventas, 1)).Single();
        Check(detail.id_surtido == 1 && detail.id_producto is null && detail.componentes.Count == 2 && detail.nombre_producto == "Surtido", "Detalle devuelve nombre y composición");
        var stored = await f.Db.ventas.AsNoTracking().Include(s => s.cliente).Include(s => s.usuario)
            .Include(s => s.DetalleVentas).ThenInclude(d => d.Surtido)
            .Include(s => s.DetalleVentas).ThenInclude(d => d.producto_presentacion).ThenInclude(p => p.Presentacion)
            .SingleAsync(s => s.id_ventas == v.id_ventas);
        var empresa = new Empresa { nombre_empresa = "Prueba", direccion = "Guatemala", ubicacion = "Guatemala" };
        var ticket = API_SISTEMA.Services.Comprobantes.TicketPos.DesdeVenta(stored, empresa);
        Check(ticket.Lineas.Single().Descripcion.Contains("Surtido"), "Ticket interno muestra el nombre del surtido sin producto ficticio");
        var imagen = API_SISTEMA.Services.Comprobantes.ComprobanteImagen.Planificar(stored, empresa);
        Check(imagen.SelectMany(p => p.Filas).Single().Celdas.Contains("Surtido"), "Comprobante de imagen conserva la línea de surtido");
        var edit = new ModificarVentaDto { id_cliente = 1, detalles = [new() { id_surtido = 1, id_producto_presentacion = 1, cantidad = 1,
            componentes = [new() { id_producto = 1, cantidad = 2 }, new() { id_producto = 2, cantidad = 4 }] }] };
        await new ActualizarVentaService(f.Db).ModificarVenta(v.id_ventas, edit, 1);
        Check((await f.Db.productos.FindAsync(1))!.Stock == 18 && (await f.Db.productos.FindAsync(2))!.Stock == 16,
            "Editar venta restaura composición anterior y descuenta la nueva");
    }
    await using (var f = await Fixture.Create())
    {
        var dto = Request(); dto.detalles[0].cantidad = 2; dto.detalles[0].componentes![0].cantidad = 7; dto.detalles[0].componentes![1].cantidad = 5;
        var v = await f.Sell(dto);
        Check(v.total == 40 && v.DetalleVentas.Single().unidades_descontadas == 12, "Dos bandejas requieren exactamente 12 unidades");
    }
    await using (var f = await Fixture.Create())
    {
        var dto = Request(); dto.detalles.Add(new() { id_producto = 1, id_producto_presentacion = 3, cantidad = 4 });
        await f.Sell(dto);
        Check((await f.Db.productos.FindAsync(1))!.Stock == 13, "Suma consumo regular y surtido de un mismo producto");
    }
    await using (var f = await Fixture.Create())
    {
        var dto = Request(); dto.detalles.Add(new() { id_surtido = 1, id_producto_presentacion = 2, cantidad = 1,
            componentes = [new() { id_producto = 1, cantidad = 6 }, new() { id_producto = 2, cantidad = 6 }] });
        Check((await f.Sell(dto)).total == 60 && (await f.Db.productos.FindAsync(1))!.Stock == 11, "Varias presentaciones del mismo surtido comparten inventario");
    }
    await using (var f = await Fixture.Create())
    {
        await f.Db.productos.Where(p => p.IdProducto == 2).ExecuteUpdateAsync(s => s.SetProperty(p => p.PrecioPromedioCompra, 2.999999m));
        Check((await f.Sell(Request())).ganancia_total == 4.010003m, "Cambios decimales dentro del mismo entero permiten venta y conservan seis decimales");
    }
    await using (var f = await Fixture.Create())
    {
        var dto = Request(); dto.detalles[0].componentes = [new() { id_producto = 1, cantidad = 6 }];
        dto.detalles[0].descuento = 6.02m;
        Check((await f.Sell(dto)).ganancia_total == 0, "Permite surtido exactamente al costo después del descuento");
    }
    await using (var f = await Fixture.Create())
    {
        await f.Db.producto_presentaciones.Where(p => p.id_producto_presentacion == 1).ExecuteUpdateAsync(s => s.SetProperty(p => p.precio, 14m));
        var dto = Request(); dto.detalles[0].componentes = [new() { id_producto = 1, cantidad = 6 }];
        Check((await f.Sell(dto)).total == 14m, "Al vender compara composición real aunque el precio ya no cubra el componente más caro");
    }
    await Case("Falta composición", d => d.detalles[0].componentes = null);
    await Case("Composición vacía", d => d.detalles[0].componentes = []);
    await Case("Componente nulo", d => d.detalles[0].componentes = [null!]);
    await Case("Producto normal con componentes", d => { d.detalles[0].id_producto = 1; d.detalles[0].id_surtido = null; d.detalles[0].id_producto_presentacion = 3; });
    await Case("Presentación normal usada como surtido", d => d.detalles[0].id_producto_presentacion = 3);
    await Case("Componente cero", d => d.detalles[0].componentes![0].cantidad = 0);
    await Case("Componente negativo", d => d.detalles[0].componentes![0].cantidad = -1);
    await Case("Producto repetido", d => d.detalles[0].componentes![1].id_producto = 1);
    await Case("Producto no permitido", d => d.detalles[0].componentes![0].id_producto = 3);
    await Case("Producto inexistente", d => d.detalles[0].componentes![0].id_producto = 999);
    await Case("Unidades incompletas", d => d.detalles[0].componentes![0].cantidad = 2);
    await Case("Exceso de unidades", d => d.detalles[0].componentes![0].cantidad = 4);
    await Case("Presentaciones cero", d => d.detalles[0].cantidad = 0);
    await Case("Desbordamiento de unidades", d => d.detalles[0].cantidad = int.MaxValue);
    await Case("Ambos propietarios", d => d.detalles[0].id_producto = 1);
    await Case("Ningún propietario", d => d.detalles[0].id_surtido = null);
    await Case("Surtido inexistente", d => d.detalles[0].id_surtido = 999);
    await Case("Presentación ajena", d => d.detalles[0].id_producto_presentacion = 4);
    await Case("Presentación inexistente", d => d.detalles[0].id_producto_presentacion = 999);
    await Case("Surtido repetido", d => d.detalles.Add(d.detalles[0]));
    await Case("Descuento negativo", d => d.detalles[0].descuento = -1);
    await Case("Descuento con tres decimales", d => d.detalles[0].descuento = .001m);
    await Case("Descuento produce pérdida", d => d.detalles[0].descuento = 5);
    await Case("No redondea costo real hacia abajo", d => d.detalles[0].descuento = 4.91m,
        db => db.productos.Where(p => p.IdProducto == 2).ExecuteUpdateAsync(s => s.SetProperty(p => p.PrecioPromedioCompra, 2.700001m)));
    await Case("Descuento igual al bruto", d => d.detalles[0].descuento = 20);
    await Case("Pago mayor que total", d => d.pago = new() { monto = 21, metodo_pago = "EFECTIVO" });
    await Case("Stock insuficiente", _ => { }, db => db.productos.Where(p => p.IdProducto == 1).ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, 2)));
    await Case("Stock agotado", _ => { }, db => db.productos.Where(p => p.IdProducto == 2).ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, 0)));
    await Case("Acumulado regular más surtido supera stock", d => d.detalles.Add(new() { id_producto = 1, id_producto_presentacion = 3, cantidad = 18 }));
    await Case("Dos surtidos consumen el mismo stock", d => d.detalles.Add(new() { id_surtido = 2, id_producto_presentacion = 4, cantidad = 3,
        componentes = [new() { id_producto = 1, cantidad = 18 }] }));
    await Case("Surtido inactivo", _ => { }, db => db.Surtidos.Where(s => s.IdSurtido == 1).ExecuteUpdateAsync(s => s.SetProperty(p => p.Estado, false)));
    await Case("Marca inactiva", _ => { }, db => db.Marcas.ExecuteUpdateAsync(s => s.SetProperty(p => p.Estado, false)));
    await Case("Categoría inactiva", _ => { }, db => db.categorias.ExecuteUpdateAsync(s => s.SetProperty(p => p.Estado, false)));
    await Case("Presentación desactivada", _ => { }, db => db.producto_presentaciones.Where(p => p.id_producto_presentacion == 1).ExecuteUpdateAsync(s => s.SetProperty(p => p.estado, false)));
    await Case("Catálogo de presentación inactivo", _ => { }, db => db.presentaciones.ExecuteUpdateAsync(s => s.SetProperty(p => p.Estado, false)));
    await Case("Costo promedio cambia de entero aunque no se use ese componente", d => d.detalles[0].componentes = [new() { id_producto = 1, cantidad = 6 }],
        db => db.productos.Where(p => p.IdProducto == 2).ExecuteUpdateAsync(s => s.SetProperty(p => p.PrecioPromedioCompra, 3.01m)));
    foreach (decimal? cost in new decimal?[] { null, 0m, -1m })
        await Case("Costo inválido " + cost, _ => { }, db => db.productos.Where(p => p.IdProducto == 2).ExecuteUpdateAsync(s => s.SetProperty(p => p.PrecioPromedioCompra, cost)));
    await Case("Caja cerrada", _ => { }, db => db.sesioncaja.ExecuteUpdateAsync(s => s.SetProperty(p => p.fecha_cierre, (DateTime?)DateTime.Now)), typeof(CajaValidationException));
    await Case("Sin permiso vender", _ => { }, async db => { db.usuario_permisos.Add(new() { id_usuario = 1, id_permiso = 1, permitido = false }); await db.SaveChangesAsync(); }, typeof(UnauthorizedAccessException));
    await Case("Fallo al insertar movimiento", d => d.pago = new() { monto = 10, metodo_pago = "EFECTIVO" },
        db => db.Database.ExecuteSqlRawAsync("CREATE TRIGGER falla_movimiento BEFORE INSERT ON movimiento_caja BEGIN SELECT RAISE(ABORT,'prueba'); END;"), typeof(DbUpdateException));
    foreach (var quantity in new[] { "1.5", "-0.5", "2147483648", "\"3\"" })
    {
        try { JsonSerializer.Deserialize<ComponenteSurtidoVentaDto>("{\"id_producto\":1,\"cantidad\":" + quantity + "}"); throw new Exception("Cantidad no entera aceptada"); }
        catch (JsonException) { Check(true, "Contrato rechaza cantidad no entera o fuera de rango: " + quantity); }
    }
    await using (var f = await Fixture.Create())
    {
        var catalog = await new API_SISTEMA.Services.CatalogoPublico.CatalogoPublicoService(f.Db).ListarSurtidosPorMarca(1);
        Check(catalog!.Surtidos.Count == 2 && catalog.Surtidos.Single(s => s.IdSurtido == 1).Productos.Count == 2, "Catálogo público incluye surtidos y productos permitidos de la marca");
        var json = JsonSerializer.Serialize(catalog);
        Check(!json.Contains("Costo") && !json.Contains("Promedio") && !json.Contains("Stock"), "Catálogo público no expone costos ni existencias exactas");
        var request = new RegistrarPedidoCatalogoDto { IdOperacion = Guid.NewGuid(), IdZonaEntrega = 1, NombreCliente = "Cliente público",
            Telefono = "+50255551234", Direccion = "Guatemala", Detalles = [new() { IdProductoPresentacion = 1, Cantidad = 1,
                Componentes = [new() { IdProducto = 1, Cantidad = 3 }, new() { IdProducto = 2, Cantidad = 3 }] }] };
        var service = new RegistrarPedidoCatalogoService(f.Db);
        await service.Registrar(request); await service.Registrar(request);
        var pedido = await f.Db.PedidosCatalogo.Include(p => p.Detalles).ThenInclude(d => d.Componentes).SingleAsync();
        Check(pedido.Total == 20 && pedido.Detalles.Single().Componentes.Count == 2 && await f.Db.PedidosCatalogo.CountAsync() == 1,
            "Pedido público guarda composición y repetirlo no duplica registros");
        Check((await f.Db.productos.FindAsync(1))!.Stock == 20, "Pedido público no reserva ni descuenta existencias");
        var altered = new RegistrarPedidoCatalogoDto { IdOperacion = request.IdOperacion, IdZonaEntrega = 1, NombreCliente = request.NombreCliente,
            Telefono = request.Telefono, Direccion = request.Direccion, Detalles = [new() { IdProductoPresentacion = 1, Cantidad = 1,
                Componentes = [new() { IdProducto = 1, Cantidad = 4 }, new() { IdProducto = 2, Cantidad = 2 }] }] };
        try { await service.Registrar(altered); throw new Exception("Reutilizó clave con otra composición"); }
        catch (PedidoCatalogoConflictoException) { Check(true, "Misma clave de pedido con otra composición devuelve conflicto"); }
        var detail = await new ListarPedidosCatalogoService(f.Db).ObtenerDetalle(pedido.IdPedido);
        Check(detail!.Detalles.Single().Componentes.Single(c => c.IdProducto == 1).Cantidad == 3, "Detalle del pedido conserva composición y nombres");
        (await f.Db.rols.SingleAsync()).nombre = "ADMINISTRADOR"; await f.Db.SaveChangesAsync();
        var http = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        http.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity([
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1")], "test"));
        var peticion = new API_SISTEMA.Security.ContextoPeticion(new Microsoft.AspNetCore.Http.HttpContextAccessor { HttpContext = http });
        var auditoria = new API_SISTEMA.Services.Auditoria.CatalogoAuditoriaService(f.Db, peticion);
        await new EditarPedidoCatalogoService(f.Db, peticion, auditoria).Editar(pedido.IdPedido, new() { Detalles = [new()
            { IdProductoPresentacion = 1, Cantidad = 1, Componentes = [new() { IdProducto = 1, Cantidad = 4 }, new() { IdProducto = 2, Cantidad = 2 }] }] });
        Check((await new ListarPedidosCatalogoService(f.Db).ObtenerDetalle(pedido.IdPedido))!.Detalles.Single().Componentes.Single(c => c.IdProducto == 1).Cantidad == 4,
            "Edición del pedido actualiza cantidades de componentes sin borrarlas");
        var confirmation = new ConfirmarVentaPedidoDto { IdOperacion = Guid.NewGuid(), TotalEsperado = 20, PagoInicial = 0,
            Detalles = [new() { IdProductoPresentacion = 1, Cantidad = 1,
                Componentes = [new() { IdProducto = 1, Cantidad = 4 }, new() { IdProducto = 2, Cantidad = 2 }] }] };
        var confirmService = new ConfirmarVentaPedidoService(f.Db, peticion,
            new CrearVentaService(f.Db, new MovimientoCajaService(f.Db), new PermisoUsuarioService(f.Db)), auditoria);
        await confirmService.Confirmar(pedido.IdPedido, confirmation); await confirmService.Confirmar(pedido.IdPedido, confirmation);
        Check(await f.Db.ventas.CountAsync() == 1 && (await f.Db.productos.FindAsync(1))!.Stock == 16 && (await f.Db.productos.FindAsync(2))!.Stock == 18,
            "Conversión de pedido a venta conserva composición y descuenta solo una vez");
    }
    foreach (var cantidades in new[] { (2, 3), (4, 3), (0, 6), (-1, 7) })
    {
        await using var f = await Fixture.Create();
        try {
            await new RegistrarPedidoCatalogoService(f.Db).Registrar(new() { IdOperacion = Guid.NewGuid(), IdZonaEntrega = 1, NombreCliente = "Cliente",
                Telefono = "+50255551234", Direccion = "Guatemala", Detalles = [new() { IdProductoPresentacion = 1, Cantidad = 1,
                    Componentes = [new() { IdProducto = 1, Cantidad = cantidades.Item1 }, new() { IdProducto = 2, Cantidad = cantidades.Item2 }] }] });
            throw new Exception("Pedido con composición inválida aceptado");
        } catch (ValidationException) { Check(await f.Db.PedidosCatalogo.CountAsync() == 0, "Pedido público rechaza composición inválida " + cantidades); }
    }
    Console.WriteLine($"{passed} comprobaciones de venta con surtidos aprobadas.");
}

sealed class Fixture(SqliteConnection connection, CheckDb db) : IAsyncDisposable
{
    public CheckDb Db => db;
    public Task<API_SISTEMA.Models.Ventas> Sell(CrearVentaDto dto) => new CrearVentaService(db, new MovimientoCajaService(db), new PermisoUsuarioService(db)).CrearVenta(dto, 1);
    public async ValueTask DisposeAsync() { await db.DisposeAsync(); await connection.DisposeAsync(); }
    public static async Task<Fixture> Create()
    {
        var connection = new SqliteConnection("Data Source=:memory:"); await connection.OpenAsync();
        connection.CreateFunction<string, string>("btrim", s => s.Trim());
        var db = new CheckDb(new DbContextOptionsBuilder<SistemaDbContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.rols.Add(new Rol { id_rol = 1, nombre = "VENDEDOR", descripcion = "Prueba", estado = true });
        db.usuarios.Add(new Usuario { id_usuario = 1, id_rol = 1, nombre = "Usuario", apellido = "Prueba", usuario = "prueba", telefono = "123", estado = true });
        db.tabla_Permisos.Add(new TablaPermiso { id_permiso = 1, nombre = "Vender", descripcion = "Prueba", estado = true });
        db.rol_Permisocs.Add(new RolPermiso { id_rol = 1, id_permiso = 1 });
        db.categorias.Add(new Categoria { IdCategoria = 1, nombreCategoria = "Bebidas", Estado = true });
        db.Marcas.Add(new Marca { IdMarca = 1, IdCategoria = 1, Nombre = "Marca", Estado = true });
        db.presentaciones.Add(new Presentacion { IdPresentacion = 1, Descripcion = "Bandeja", Estado = true });
        db.productos.AddRange(Enumerable.Range(1, 3).Select(id => new Productos { IdProducto = id, IdMarca = 1, Nombre = "Sabor " + id, Stock = 20, PrecioPromedioCompra = id == 1 ? 2.33m : id == 2 ? 2.70m : 2.50m }));
        db.Surtidos.AddRange(new Surtido { IdSurtido = 1, IdMarca = 1, Nombre = "Surtido", Productos = [new() { IdProducto = 1 }, new() { IdProducto = 2 }] },
            new Surtido { IdSurtido = 2, IdMarca = 1, Nombre = "Otro surtido", Productos = [new() { IdProducto = 1 }] });
        db.producto_presentaciones.AddRange(new ProductoPresentacion { id_producto_presentacion = 1, id_surtido = 1, IdPresentacion = 1, unidades_equivalentes = 6, precio = 20, estado = true },
            new ProductoPresentacion { id_producto_presentacion = 2, id_surtido = 1, IdPresentacion = 1, unidades_equivalentes = 12, precio = 40, estado = true },
            new ProductoPresentacion { id_producto_presentacion = 3, id_producto = 1, IdPresentacion = 1, unidades_equivalentes = 1, precio = 5, estado = true },
            new ProductoPresentacion { id_producto_presentacion = 4, id_surtido = 2, IdPresentacion = 1, unidades_equivalentes = 6, precio = 20, estado = true });
        db.cliente.Add(new Cliente { id_cliente = 1, nombre = "Cliente", estado = true });
        db.estado_venta.AddRange(new EstadoVenta { id_estado_venta = 1, descripcion = "Pagada", estado = true }, new EstadoVenta { id_estado_venta = 2, descripcion = "Pendiente", estado = true });
        db.caja.Add(new Caja { id_caja = 1, nombre_caja = "Caja", estado = true });
        db.sesioncaja.Add(new SesionCaja { id_sesion_caja = 1, id_caja = 1, id_usuario_apertura = 1, fecha_apertura = DateTime.Now });
        db.tipomovimientocaja.Add(new TipoMovimientoCaja { id_tipo_movimiento = TiposMovimientoCaja.Venta, nombre_movimiento = "Venta", naturaleza = "ENTRADA" });
        db.ZonasEntrega.Add(new ZonaEntrega { IdZonaEntrega = 1, Nombre = "Centro", MinimoCompra = 1, Estado = true });
        await db.SaveChangesAsync(); db.ChangeTracker.Clear(); return new(connection, db);
    }
}

sealed class CheckDb(DbContextOptions<SistemaDbContext> options) : SistemaDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        var keep = new HashSet<Type> { typeof(PedidoCatalogo), typeof(DetallePedidoCatalogo), typeof(DetallePedidoSurtido), typeof(ZonaEntrega), typeof(AuditoriaEvento), typeof(AuditoriaEventoDetalle), typeof(Usuario), typeof(Rol), typeof(RolPermiso), typeof(TablaPermiso), typeof(UsuarioPermiso),
            typeof(Productos), typeof(Marca), typeof(Categoria), typeof(ProductoPresentacion), typeof(Presentacion), typeof(Cliente),
            typeof(API_SISTEMA.Models.Ventas), typeof(DetalleVenta), typeof(EstadoVenta), typeof(Pagos), typeof(SesionCaja), typeof(Caja),
            typeof(MovimientoCaja), typeof(TipoMovimientoCaja), typeof(VentaOperacion), typeof(Surtido), typeof(SurtidoProducto), typeof(DetalleVentaSurtido), typeof(DocumentoFel) };
        foreach (var e in b.Model.GetEntityTypes().ToArray()) if (!keep.Contains(e.ClrType)) b.Ignore(e.ClrType);
        b.Entity<DocumentoFel>().Property(d => d.version).HasDefaultValue(0u);
    }
}
