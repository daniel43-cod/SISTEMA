using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.PedidosCatalogo;
using API_SISTEMA.Models;
using API_SISTEMA.Services.PedidosCatalogo;
using Microsoft.EntityFrameworkCore;

internal static class PedidosCatalogoChecks
{
    public static async Task Ejecutar(DbContextOptions<SistemaDbContext> options, Action<bool, string> check)
    {
        await using (var db = new CheckDb(options))
        {
            db.ZonasEntrega.AddRange(
                new ZonaEntrega { IdZonaEntrega = 1, Nombre = "Centro", MinimoCompra = 200 },
                new ZonaEntrega { IdZonaEntrega = 2, Nombre = "Limite", MinimoCompra = 260 },
                new ZonaEntrega { IdZonaEntrega = 3, Nombre = "Inactiva", MinimoCompra = 200, Estado = false });
            db.producto_presentaciones.Add(new ProductoPresentacion { id_producto_presentacion = 200, id_producto = 1,
                IdPresentacion = 1, unidades_equivalentes = 2, precio = 250, estado = true });
            await db.SaveChangesAsync();
        }
        RegistrarPedidoCatalogoDto Dto(Guid? operacion = null, int zona = 1, int cantidad = 2, string nombre = "Cliente prueba",
            List<RegistrarDetallePedidoCatalogoDto>? detalles = null, string telefono = "+50255551234") => new()
        {
            IdOperacion = operacion ?? Guid.NewGuid(), NombreCliente = nombre, Telefono = telefono,
            Direccion = "Calle de prueba", IdZonaEntrega = zona,
            Detalles = detalles ?? [new() { IdProductoPresentacion = 1, Cantidad = cantidad }]
        };
        async Task<PedidoCatalogoRegistradoDto> Registrar(RegistrarPedidoCatalogoDto dto)
        {
            await using var db = new CheckDb(options);
            return await new RegistrarPedidoCatalogoService(db).Registrar(dto);
        }
        async Task Rechazar(RegistrarPedidoCatalogoDto dto, string mensaje)
        {
            try { await Registrar(dto); throw new Exception("Pedido inválido aceptado: " + mensaje); }
            catch (ValidationException) { check(true, mensaje); }
        }
        await using var consulta = new CheckDb(options);
        var stock = await consulta.productos.AsNoTracking().Where(p => p.IdProducto == 1).Select(p => p.Stock).SingleAsync();
        var ventas = await consulta.ventas.CountAsync();
        var pagos = await consulta.pagos.CountAsync();
        var movimientos = await consulta.movimientocaja.CountAsync();
        var zonas = await new RegistrarPedidoCatalogoService(consulta).ListarZonas();
        check(zonas.Count == 2 && zonas.All(z => z.IdZonaEntrega != 3), "Pedidos: solo zonas activas");
        await Rechazar(Dto(cantidad: 1), "Pedidos: rechaza total menor al mínimo");
        await Rechazar(Dto(zona: 2), "Pedidos: rechaza total igual al mínimo");
        await Rechazar(Dto(zona: 3), "Pedidos: rechaza zona inactiva");
        await Rechazar(Dto(zona: 999), "Pedidos: rechaza zona inexistente");
        await Rechazar(Dto(cantidad: int.MaxValue), "Pedidos: rechaza falta de stock");
        await Rechazar(Dto(nombre: "  "), "Pedidos: exige nombre");
        foreach (var telefono in new[] { "", "55551234", "+50355551234", "+5025555123", "+502555512345", "+502 55551234", "+5025555-1234", "+502abcdefgh", "+50255551234\n" })
            await Rechazar(Dto(telefono: telefono), "Pedidos: rechaza teléfono fuera del formato guatemalteco");
        await Rechazar(Dto(detalles: [new() { IdProductoPresentacion = 1, Cantidad = 1 }, new() { IdProductoPresentacion = 1, Cantidad = 1 }]), "Pedidos: rechaza presentaciones duplicadas");
        await Rechazar(Dto(detalles: [new() { IdProductoPresentacion = 1, Cantidad = 50 }, new() { IdProductoPresentacion = 200, Cantidad = 25 }]), "Pedidos: acumula stock entre presentaciones");
        await consulta.producto_presentaciones.Where(p => p.id_producto_presentacion == 1).ExecuteUpdateAsync(s => s.SetProperty(p => p.precio, 0));
        await Rechazar(Dto(), "Pedidos: rechaza precio cero");
        await consulta.producto_presentaciones.Where(p => p.id_producto_presentacion == 1).ExecuteUpdateAsync(s => s.SetProperty(p => p.precio, 100));
        await Rechazar(Dto(), "Pedidos: rechaza precio al costo");
        await consulta.producto_presentaciones.Where(p => p.id_producto_presentacion == 1).ExecuteUpdateAsync(s => s.SetProperty(p => p.precio, 130));
        await consulta.Marcas.Where(m => m.IdMarca == 1).ExecuteUpdateAsync(s => s.SetProperty(m => m.Estado, false));
        await Rechazar(Dto(), "Pedidos: rechaza marca inactiva");
        await consulta.Marcas.Where(m => m.IdMarca == 1).ExecuteUpdateAsync(s => s.SetProperty(m => m.Estado, true));
        var dto = Dto();
        var respuesta = await Registrar(dto);
        var repetida = await Registrar(dto);
        check(respuesta == repetida && respuesta.Total == 260 && respuesta.Estado == "Pendiente", "Pedidos: calcula total y permite reintento idéntico");
        try { await Registrar(Dto(dto.IdOperacion, cantidad: 3)); throw new Exception("Clave reutilizada aceptada"); }
        catch (PedidoCatalogoConflictoException) { check(true, "Pedidos: rechaza misma clave con otros datos"); }
        var pedido = await consulta.PedidosCatalogo.AsNoTracking().Include(p => p.Detalles).SingleAsync();
        check(pedido.IdVenta is null && pedido.Detalles.Single().Subtotal == 260 && pedido.FechaUtc != default,
            "Pedidos: guarda cabecera, detalle calculado y fecha sin venta");
        check(await consulta.productos.Where(p => p.IdProducto == 1).Select(p => p.Stock).SingleAsync() == stock &&
            await consulta.ventas.CountAsync() == ventas && await consulta.pagos.CountAsync() == pagos &&
            await consulta.movimientocaja.CountAsync() == movimientos, "Pedidos: no modifica inventario, ventas, pagos ni caja");
        await consulta.Database.ExecuteSqlRawAsync("CREATE TRIGGER rechazo_detalle_pedido BEFORE INSERT ON detalle_pedido_catalogo BEGIN SELECT RAISE(ABORT, 'fallo de prueba'); END;");
        try { await Registrar(Dto()); throw new Exception("Pedido incompleto aceptado"); }
        catch (DbUpdateException) { check(true, "Pedidos: fallo del detalle rechaza operación"); }
        check(await consulta.PedidosCatalogo.CountAsync() == 1 && await consulta.DetallesPedidoCatalogo.CountAsync() == 1,
            "Pedidos: transacción revierte cabecera cuando falla el detalle");
        await consulta.Database.ExecuteSqlRawAsync("DROP TRIGGER rechazo_detalle_pedido;");
        var listado = new ListarPedidosCatalogoService(consulta);
        var detalle = await listado.ObtenerDetalle(pedido.IdPedido);
        check(detalle is not null && detalle.NombreCliente == dto.NombreCliente && detalle.Telefono == dto.Telefono &&
            detalle.Direccion == dto.Direccion && detalle.ZonaEntrega == "Centro" && detalle.Estado == "Pendiente" &&
            detalle.Total == 260 && detalle.Detalles.Count == 1 && detalle.Detalles[0].Cantidad == 2 &&
            detalle.Detalles[0].PrecioUnitario == 130 && detalle.Detalles[0].Subtotal == 260,
            "Detalle de pedido: devuelve cabecera y productos con importes guardados");
        await consulta.producto_presentaciones.Where(p => p.id_producto_presentacion == 1)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.precio, 140));
        check((await listado.ObtenerDetalle(pedido.IdPedido))!.Detalles[0].PrecioUnitario == 130,
            "Detalle de pedido: conserva precio histórico aunque cambie catálogo");
        await consulta.producto_presentaciones.Where(p => p.id_producto_presentacion == 1)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.precio, 130));
        check(await listado.ObtenerDetalle(int.MaxValue) is null, "Detalle de pedido: inexistente devuelve null");
        foreach (var id in new[] { 0, -1 })
        {
            try { await listado.ObtenerDetalle(id); throw new Exception("ID inválido aceptado"); }
            catch (ValidationException) { check(true, "Detalle de pedido: valida identificador"); }
        }
        using (var cancel = new CancellationTokenSource())
        {
            cancel.Cancel();
            try { await listado.ObtenerDetalle(pedido.IdPedido, cancel.Token); throw new Exception("Cancelación ignorada"); }
            catch (OperationCanceledException) { check(true, "Detalle de pedido: respeta cancelación"); }
        }
        var controladorDetalle = new API_SISTEMA.Controllers.PedidoCatalogoController(new RegistrarPedidoCatalogoService(consulta));
        check(await controladorDetalle.Detalle(int.MaxValue, listado) is Microsoft.AspNetCore.Mvc.NotFoundObjectResult,
            "Detalle de pedido: endpoint devuelve 404 para inexistente");
        var accionDetalle = typeof(API_SISTEMA.Controllers.PedidoCatalogoController).GetMethod("Detalle")!;
        check(!accionDetalle.IsDefined(typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute), true) &&
            accionDetalle.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true)
                .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Single().Roles == API_SISTEMA.Utilidades.Roles.Administrador,
            "Detalle de pedido: acceso exclusivo para administradores");
        var inicial = await listado.ListarPendientes();
        check(inicial.Pedidos.Count == 1 && inicial.Pedidos[0].ZonaEntrega == "Centro" &&
            inicial.Pedidos[0].Telefono == "+50255551234" && inicial.Pedidos[0].Total == 260 && !inicial.HayMas,
            "Pendientes: devuelve contacto, zona y total del pedido");
        var fecha = DateTime.UtcNow.AddMinutes(1);
        for (int i = 0; i < 51; i++)
            consulta.PedidosCatalogo.Add(new PedidoCatalogo { IdOperacion = Guid.NewGuid(), NombreCliente = "Pedido " + i,
                Telefono = "+50255551234", Direccion = "Prueba", IdZonaEntrega = 1, Total = 260, FechaUtc = fecha });
        foreach (var estado in new[] { EstadoPedidoCatalogo.Aceptado, EstadoPedidoCatalogo.Rechazado, EstadoPedidoCatalogo.Cancelado })
            consulta.PedidosCatalogo.Add(new PedidoCatalogo { IdOperacion = Guid.NewGuid(), NombreCliente = "Otro estado",
                Telefono = "+50255551234", Direccion = "Prueba", IdZonaEntrega = 1, Total = 260, Estado = estado, FechaUtc = fecha.AddHours(1) });
        await consulta.SaveChangesAsync();
        consulta.ChangeTracker.Clear();
        var primera = await listado.ListarPendientes();
        var segunda = await listado.ListarPendientes(2);
        check(primera.Pedidos.Count == 50 && primera.HayMas && primera.TamanoPagina == 50 &&
            segunda.Pedidos.Count == 2 && !segunda.HayMas, "Pendientes: pagina de 50 y detecta última página");
        check(primera.Pedidos.Concat(segunda.Pedidos).All(p => p.NombreCliente != "Otro estado") &&
            primera.Pedidos.Select(p => p.IdPedido).Intersect(segunda.Pedidos.Select(p => p.IdPedido)).Count() == 0,
            "Pendientes: excluye otros estados y no repite pedidos");
        check(primera.Pedidos.Take(49).Zip(primera.Pedidos.Skip(1)).All(p => p.First.IdPedido > p.Second.IdPedido),
            "Pendientes: orden estable por fecha e identificador descendentes");
        check(!consulta.ChangeTracker.Entries<PedidoCatalogo>().Any(), "Pendientes: consulta sin tracking");
        check((await listado.ListarPendientes(3)).Pedidos.Count == 0, "Pendientes: página vacía válida");
        foreach (var pagina in new[] { 0, -1, 100001 })
        {
            try { await listado.ListarPendientes(pagina); throw new Exception("Página inválida aceptada"); }
            catch (ValidationException) { check(true, "Pendientes: valida página"); }
        }
        var controller = typeof(API_SISTEMA.Controllers.PedidoCatalogoController);
        var action = controller.GetMethod("Pendientes")!;
        check(!controller.IsDefined(typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute), true) &&
            !action.IsDefined(typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute), true) &&
            action.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true)
                .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Single().Roles == API_SISTEMA.Utilidades.Roles.Administrador,
            "Pendientes: autorización exclusiva de administrador sin AllowAnonymous heredado");
        check(controller.GetMethod("Registrar")!.IsDefined(typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute), true) &&
            controller.GetMethod("Zonas")!.IsDefined(typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute), true),
            "Pedidos: registro y zonas conservan acceso público");
    }
}
