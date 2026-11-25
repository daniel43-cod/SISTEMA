using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Ventas;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Ventas;
using API_SISTEMA.Services.MovimientoCaja;
using API_SISTEMA.Services.Permisos;
using Microsoft.EntityFrameworkCore;
using Npgsql;

static class SurtidoVentaPostgresChecks
{
    public static async Task Run(DbContextOptions<SistemaDbContext> options, int usuario, int cliente)
    {
        await using var db = new SistemaDbContext(options);
        var marca = await db.Marcas.FirstAsync();
        var presentacion = await db.presentaciones.FirstAsync();
        var p1 = new Productos { IdMarca = marca.IdMarca, Nombre = "Surtido PG limón", Stock = 3, PrecioPromedioCompra = 2.33m };
        var p2 = new Productos { IdMarca = marca.IdMarca, Nombre = "Surtido PG uva", Stock = 3, PrecioPromedioCompra = 2.70m };
        db.productos.AddRange(p1, p2); await db.SaveChangesAsync();
        var surtido = new Surtido { Nombre = "Prueba concurrencia surtido", IdMarca = marca.IdMarca,
            Productos = [new() { IdProducto = p1.IdProducto }, new() { IdProducto = p2.IdProducto }] };
        var pr = new ProductoPresentacion { Surtido = surtido, IdPresentacion = presentacion.IdPresentacion, unidades_equivalentes = 6, precio = 20, estado = true };
        db.producto_presentaciones.Add(pr); await db.SaveChangesAsync();
        CrearVentaDto Dto() => new() { id_operacion = Guid.NewGuid(), id_cliente = cliente,
            detalles = [new() { id_surtido = surtido.IdSurtido, id_producto_presentacion = pr.id_producto_presentacion, cantidad = 1,
                componentes = [new() { id_producto = p1.IdProducto, cantidad = 3 }, new() { id_producto = p2.IdProducto, cantidad = 3 }] }] };
        async Task<API_SISTEMA.Models.Ventas?> Sell(CrearVentaDto dto)
        {
            for (var attempt = 0; ; attempt++)
            {
                await using var context = new SistemaDbContext(options);
                try { return await new CrearVentaService(context, new MovimientoCajaService(context), new PermisoUsuarioService(context)).CrearVenta(dto, usuario); }
                catch (Exception e) when (attempt < 3 && e.GetBaseException() is PostgresException { SqlState: PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.DeadlockDetected }) { }
                catch (ValidationException) { return null; }
            }
        }
        var first = Dto(); var second = Dto();
        var results = await Task.WhenAll(Sell(first), Sell(second));
        if (results.Count(r => r is not null) != 1) throw new Exception("Dos ventas simultáneas consumieron el mismo stock de surtido.");
        db.ChangeTracker.Clear();
        if (await db.productos.Where(p => p.IdProducto == p1.IdProducto || p.IdProducto == p2.IdProducto).AnyAsync(p => p.Stock != 0))
            throw new Exception("Existencias inconsistentes después de concurrencia.");
        Console.WriteLine("PG OK Dos ventas simultáneas de surtido: solo una consume las últimas existencias");
        var original = results[0] is not null ? first : second;
        var repeat = await Sell(original);
        if (repeat?.id_ventas != results.Single(r => r is not null)!.id_ventas || repeat.ganancia_total != 4.91m)
            throw new Exception("Idempotencia o ganancia de surtido incorrecta en PostgreSQL.");
        Console.WriteLine("PG OK Reintento conserva venta y costos históricos sin descontar inventario otra vez");
        var catalogo = await new VentaService(db).ListarCatalogoSurtidos();
        if (!catalogo.Any(s => s.id_surtido == surtido.IdSurtido && s.componentes.Count == 2)) throw new Exception("Catálogo de surtidos PostgreSQL incorrecto.");
        Console.WriteLine("PG OK Catálogo de surtidos incluye todos sus componentes");
        await db.productos.Where(p => p.IdProducto == p2.IdProducto).ExecuteUpdateAsync(s => s.SetProperty(p => p.PrecioPromedioCompra, 3.01m));
        if ((await new VentaService(db).ListarCatalogoSurtidos()).Single(s => s.id_surtido == surtido.IdSurtido).bloqueo is null)
            throw new Exception("El catálogo no informa bloqueo por costo incompatible.");
        Console.WriteLine("PG OK Cambio de entero del costo bloquea el surtido completo");
        await using var invalid = new SistemaDbContext(options);
        invalid.detalle_Ventas.Add(new DetalleVenta { id_venta = repeat!.id_ventas, id_producto = p1.IdProducto,
            id_surtido = surtido.IdSurtido, id_producto_presentacion = pr.id_producto_presentacion });
        try { await invalid.SaveChangesAsync(); throw new Exception("Base aceptó producto y surtido en una misma línea."); }
        catch (DbUpdateException e) when (e.GetBaseException() is PostgresException { SqlState: PostgresErrorCodes.CheckViolation })
        { Console.WriteLine("PG OK Restricción de base impide ambos propietarios en la línea"); }

        await db.productos.Where(p => p.IdProducto == p1.IdProducto || p.IdProducto == p2.IdProducto)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, 6).SetProperty(p => p.PrecioPromedioCompra, 2.50m));
        var zona = await db.ZonasEntrega.FirstAsync();
        var request = new API_SISTEMA.Dtos.PedidosCatalogo.RegistrarPedidoCatalogoDto { IdOperacion = Guid.NewGuid(), IdZonaEntrega = zona.IdZonaEntrega,
            NombreCliente = "Cliente surtido público PG", Telefono = "+50255551234", Direccion = "Guatemala",
            Detalles = [new() { IdProductoPresentacion = pr.id_producto_presentacion, Cantidad = 1,
                Componentes = [new() { IdProducto = p1.IdProducto, Cantidad = 3 }, new() { IdProducto = p2.IdProducto, Cantidad = 3 }] }] };
        var registro = new API_SISTEMA.Services.PedidosCatalogo.RegistrarPedidoCatalogoService(db);
        await registro.Registrar(request); await registro.Registrar(request);
        var pedido = await db.PedidosCatalogo.Include(p => p.Detalles).ThenInclude(d => d.Componentes).SingleAsync(p => p.IdOperacion == request.IdOperacion);
        if (pedido.Detalles.Single().Componentes.Count != 2 || await db.productos.Where(p => p.IdProducto == p1.IdProducto).Select(p => p.Stock).SingleAsync() != 6)
            throw new Exception("Pedido de surtido pierde composición o descuenta stock antes de convertir.");
        Console.WriteLine("PG OK Pedido público conserva composición y no reserva existencias");
        var publico = await new API_SISTEMA.Services.CatalogoPublico.CatalogoPublicoService(db).ListarSurtidosPorMarca(marca.IdMarca);
        if (!publico!.Surtidos.Any(s => s.IdSurtido == surtido.IdSurtido && s.Productos.Count == 2)) throw new Exception("Catálogo público omite el surtido.");
        Console.WriteLine("PG OK Catálogo público de marca incluye surtido con sus productos permitidos");
        var http = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        http.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity([
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.ToString())], "test"));
        var peticion = new API_SISTEMA.Security.ContextoPeticion(new Microsoft.AspNetCore.Http.HttpContextAccessor { HttpContext = http });
        var confirmar = new API_SISTEMA.Services.PedidosCatalogo.ConfirmarVentaPedidoService(db, peticion,
            new CrearVentaService(db, new MovimientoCajaService(db), new PermisoUsuarioService(db)), new API_SISTEMA.Services.Auditoria.CatalogoAuditoriaService(db, peticion));
        await confirmar.Confirmar(pedido.IdPedido, new() { IdOperacion = Guid.NewGuid(), TotalEsperado = 20, PagoInicial = 0,
            Detalles = [new() { IdProductoPresentacion = pr.id_producto_presentacion, Cantidad = 1, Componentes = request.Detalles[0].Componentes }] });
        if (await db.productos.Where(p => p.IdProducto == p1.IdProducto).Select(p => p.Stock).SingleAsync() != 3)
            throw new Exception("Confirmación de pedido no descuenta los componentes elegidos.");
        Console.WriteLine("PG OK Pedido surtido se convierte en venta con inventario exacto");
    }
}
