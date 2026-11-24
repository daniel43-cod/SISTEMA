using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Data;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Caja;
using API_SISTEMA.Services.Ventas;
using API_SISTEMA.Services.Compras;
using API_SISTEMA.Services.PagoCompra;
using API_SISTEMA.Services.MovimientoCaja;
using API_SISTEMA.Services.Permisos;
using API_SISTEMA.Services.Auditoria;
using API_SISTEMA.Security;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Npgsql;

static class PostgreSqlIntegration
{
    public static async Task Run(string connection)
    {
        var target = new NpgsqlConnectionStringBuilder(connection);
        if (target.Host != "127.0.0.1" || target.Port != 55438 || target.Database != "sistema_checks")
            throw new InvalidOperationException("Solo se permite el clúster temporal local del puerto 55438 y base sistema_checks.");
        var options = new DbContextOptionsBuilder<SistemaDbContext>().UseNpgsql(connection, p => p.SetPostgresVersion(18, 0)).Options;
        await using var db = new SistemaDbContext(options);
        await db.Database.MigrateAsync();
        if (await db.usuarios.AnyAsync()) throw new Exception("La base temporal debe estar vacía de usuarios.");
        await db.Database.ExecuteSqlRawAsync(await File.ReadAllTextAsync("docs/sql/datos-iniciales.sql"));
        var count = 0;
        void Check(bool ok, string message) { if (!ok) throw new Exception(message); count++; Console.WriteLine("PG OK " + message); }
        var admin = new Usuario { id_rol = 1, nombre = "Admin", apellido = "Prueba", usuario = "AdminChecks", telefono = "55550001", correo = "checks@example.test", password = BCrypt.Net.BCrypt.HashPassword("ChecksPassword123"), estado = true, fecha_Creacion = RelojNegocio.Ahora() };
        db.usuarios.Add(admin);
        db.caja.Add(new Caja { nombre_caja = "Prueba temporal", estado = true });
        db.categorias.Add(new Categoria { nombreCategoria = "Prueba", Estado = true, FechaCreacion = RelojNegocio.Ahora() });
        db.presentaciones.Add(new Presentacion { Descripcion = "Unidad", Estado = true });
        db.cliente.Add(new Cliente { nombre = "Prueba", estado = true });
        db.proveedores.Add(new Proveedores { nombre = "Proveedor de prueba", telefono = "55550002", fecha_creacion = RelojNegocio.Ahora() });
        await db.SaveChangesAsync();
        Check(await db.usuarios.AnyAsync(u => u.usuario == "adminchecks" && u.correo == "CHECKS@EXAMPLE.TEST"), "Identidad insensible a mayúsculas");
        var categoria = await db.categorias.SingleAsync();
        var marca = new Marca { Nombre = "Marca", Estado = true, IdCategoria = categoria.IdCategoria };
        db.Marcas.Add(marca); await db.SaveChangesAsync();
        var producto = new Productos { Nombre = "Producto", CodigoBarra = "001234", IdMarca = marca.IdMarca, Stock = 20, StockMinimo = 2, PrecioPromedioCompra = 5.123456m, FechaCreacion = RelojNegocio.Ahora() };
        db.productos.Add(producto); await db.SaveChangesAsync();
        var presentacion = new ProductoPresentacion { id_producto = producto.IdProducto, IdPresentacion = (await db.presentaciones.SingleAsync()).IdPresentacion, unidades_equivalentes = 1, precio = 12m, estado = true };
        db.producto_presentaciones.Add(presentacion); await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Check((await db.productos.SingleAsync()).PrecioPromedioCompra == 5.123456m, "Costo conserva seis decimales");
        var http = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, admin.id_usuario.ToString())], "checks")) };
        CatalogoAuditoriaService Auditoria(SistemaDbContext context) => new(context, new ContextoPeticion(new HttpContextAccessor { HttpContext = http }));
        var caja = await new CajaService(db, Auditoria(db), new CajaSaldoService(db)).AbrirCaja(new() { id_caja = (await db.caja.SingleAsync()).id_caja, monto_inicial = 100 }, admin.id_usuario);
        Check(caja.fecha_apertura.Kind == DateTimeKind.Unspecified, "Caja conserva hora local Guatemala");
        await using (var other = new SistemaDbContext(options))
        {
            other.sesioncaja.Add(new SesionCaja { id_caja = caja.id_caja, id_usuario_apertura = admin.id_usuario, fecha_apertura = RelojNegocio.Ahora() });
            try { await other.SaveChangesAsync(); throw new Exception("Segunda caja abierta aceptada"); }
            catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }) { Check(true, "Base impide dos cajas abiertas"); }
        }
        var cliente = await db.cliente.SingleAsync();
        var venta = await new CrearVentaService(db, new MovimientoCajaService(db), new PermisoUsuarioService(db)).CrearVenta(new() {
            id_operacion = Guid.NewGuid(), id_cliente = cliente.id_cliente,
            detalles = [new() { id_producto = producto.IdProducto, id_producto_presentacion = presentacion.id_producto_presentacion, cantidad = 2, descuento = 0 }]
        }, admin.id_usuario);
        Check(venta.total == 24 && venta.saldo_pendiente == 24, "Venta y saldo");
        var clave = Guid.NewGuid();
        async Task<Pagos> Abonar(decimal monto = 5)
        {
            await using var c = new SistemaDbContext(options);
            return await new AbonarSaldoVentaService(c, new MovimientoCajaService(c), Auditoria(c)).AbonarVenta(new() { id_operacion = clave, id_venta = venta.id_ventas, monto = monto, observacion = "Prueba" }, admin.id_usuario);
        }
        async Task<Pagos> AbonarConReintento()
        {
            try { return await Abonar(); }
            catch (Exception e) when (e.GetBaseException() is PostgresException { SqlState: PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.DeadlockDetected }) { return await Abonar(); }
        }
        var concurrentes = await Task.WhenAll(AbonarConReintento(), AbonarConReintento());
        Check(concurrentes[0].id_pago == concurrentes[1].id_pago && await db.pagos.CountAsync(p => p.id_operacion == clave) == 1, "Abonos simultáneos no se duplican");
        try { await Abonar(6); throw new Exception("Clave con otros datos aceptada"); }
        catch (VentaConflictoException) { Check(true, "Clave reutilizada con otros datos rechazada"); }
        db.ChangeTracker.Clear();
        Check((await db.ventas.SingleAsync()).saldo_pendiente == 19 && (await db.productos.SingleAsync()).Stock == 18, "Saldo e inventario coherentes");
        var compra = await new CrearCompraService(db, new MovimientoCajaService(db)).CrearCompra(new() {
            id_proveedor = (await db.proveedores.SingleAsync()).id_proveedor, monto_pagado = 0,
            detalle_compra = [new() { id_producto = producto.IdProducto, cantidad = 2, precio = 10m }]
        }, admin.id_usuario);
        Check(compra.TotalCompra > 0 && compra.SaldoPendiente == compra.TotalCompra, "Compra y saldo pendiente");
        var compraClave = Guid.NewGuid();
        var pagosCompra = new PagoCompraService(db, new MovimientoCajaService(db), Auditoria(db));
        var abonoCompra = await pagosCompra.AbonarCompra(new() { IdOperacion = compraClave, id_compra = compra.IdCompra, monto = 1 }, admin.id_usuario);
        var repetidoCompra = await pagosCompra.AbonarCompra(new() { IdOperacion = compraClave, id_compra = compra.IdCompra, monto = 1 }, admin.id_usuario);
        Check(abonoCompra.id_pagos_compra == repetidoCompra.id_pagos_compra, "Abono de compra idempotente");
        var zona = new ZonaEntrega { Nombre = "Zona prueba", Estado = true, MinimoCompra = 1 };
        db.ZonasEntrega.Add(zona); await db.SaveChangesAsync();
        var solicitudPedido = new API_SISTEMA.Dtos.PedidosCatalogo.RegistrarPedidoCatalogoDto {
            IdOperacion = Guid.NewGuid(), NombreCliente = "Cliente", Telefono = "+50255550003", Direccion = "Dirección", IdZonaEntrega = zona.IdZonaEntrega,
            Detalles = [new() { IdProductoPresentacion = presentacion.id_producto_presentacion, Cantidad = 2 }] };
        var registrarPedido = new API_SISTEMA.Services.PedidosCatalogo.RegistrarPedidoCatalogoService(db);
        await registrarPedido.Registrar(solicitudPedido);
        await registrarPedido.Registrar(solicitudPedido);
        var pedido = await db.PedidosCatalogo.Include(p => p.Detalles).SingleAsync();
        Check(await db.PedidosCatalogo.CountAsync() == 1, "Servicio de pedidos idempotente");
        Check(pedido.Detalles[0].Subtotal == 24 && pedido.FechaUtc.Kind == DateTimeKind.Utc, "Subtotal calculado y fecha UTC del pedido");
        var sesion = new SesionUsuario { IdSesion = Guid.NewGuid(), IdUsuario = admin.id_usuario, TokenHash = new byte[32], FechaCreacion = DateTime.UtcNow, FechaVencimiento = DateTime.UtcNow.AddHours(1) };
        db.SesionesUsuario.Add(sesion); await db.SaveChangesAsync();
        await using (var other = new SistemaDbContext(options))
        {
            var stale = await other.SesionesUsuario.SingleAsync();
            sesion.FechaRevocacion = DateTime.UtcNow; sesion.MotivoCierre = "VOLUNTARIO"; await db.SaveChangesAsync();
            stale.MotivoCierre = "EXPIRACION";
            try { await other.SaveChangesAsync(); throw new Exception("Escritura obsoleta aceptada"); }
            catch (DbUpdateConcurrencyException) { Check(true, "xmin detecta actualizaciones simultáneas"); }
        }
        // El mismo FOR UPDATE utilizado por FEL debe bloquear la fila y liberar al terminar la transacción.
        await using (var c1 = new SistemaDbContext(options))
        await using (var tx1 = await c1.Database.BeginTransactionAsync())
        await using (var c2 = new SistemaDbContext(options))
        await using (var tx2 = await c2.Database.BeginTransactionAsync())
        {
            await c1.ventas.FromSqlInterpolated($"SELECT * FROM public.ventas WHERE id_ventas = {venta.id_ventas} FOR UPDATE").SingleAsync();
            try { await c2.ventas.FromSqlInterpolated($"SELECT * FROM public.ventas WHERE id_ventas = {venta.id_ventas} FOR UPDATE NOWAIT").SingleAsync(); throw new Exception("Bloqueo de venta omitido"); }
            catch (Exception e) when (e.GetBaseException() is PostgresException { SqlState: PostgresErrorCodes.LockNotAvailable }) { Check(true, "Bloqueo exclusivo de venta para FEL"); }
            await tx2.RollbackAsync(); await tx1.RollbackAsync();
        }
        await new API_SISTEMA.Services.Gastos.CrearGastosService(db, new MovimientoCajaService(db)).CrearGasto(new() { descripcion = "Gasto de prueba", monto = 2m }, admin.id_usuario);
        var reporteVentas = await new ReporteVentasIntervaloService(db, TimeProvider.System).ConsultarAcumulado(admin.id_usuario, caja.id_sesion_caja);
        var reporteCompras = await new ReporteComprasService(db, TimeProvider.System).ConsultarCajaActiva(admin.id_usuario);
        var reporteGastos = await new API_SISTEMA.Services.Gastos.ReporteGastosService(db, TimeProvider.System).ConsultarCajaActiva(admin.id_usuario);
        Check(reporteVentas.TotalVentas == 24m && reporteCompras!.TotalCompras == compra.TotalCompra && reporteGastos!.TotalGastos == 2m, "Reportes de ventas, compras y gastos ejecutados en PostgreSQL");
        await using (var txSaldo = await db.Database.BeginTransactionAsync())
        {
            var saldo = await new CajaSaldoService(db).Calcular(caja.id_sesion_caja, caja.monto_inicial);
            Check(saldo.Esperado == 102m, "Cuadre de caja suma entradas y salidas reales");
            await txSaldo.RollbackAsync();
        }
        var fechaFiscal = new DateTimeOffset(2026, 11, 22, 10, 30, 0, TimeSpan.FromHours(-6));
        var documento = new DocumentoFel { id_venta = venta.id_ventas, id_usuario = admin.id_usuario, ambiente = "PRUEBAS", fecha_emision = fechaFiscal };
        db.DocumentosFel.Add(documento); await db.SaveChangesAsync();
        var intento = new DocumentoFelIntento { id_documento_fel = documento.id_documento_fel, id_usuario = admin.id_usuario, identificador = Guid.NewGuid(), fecha_emision = fechaFiscal, fecha_respuesta = fechaFiscal.AddMinutes(1) };
        db.DocumentosFelIntentos.Add(intento); await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        var fiscalGuardado = await db.DocumentosFel.SingleAsync();
        Check(fiscalGuardado.fecha_emision == fechaFiscal && fiscalGuardado.fecha_emision.Offset == TimeSpan.Zero, "Fecha fiscal conserva el instante al normalizar a UTC");
        Check((await db.DocumentosFelIntentos.SingleAsync()).fecha_respuesta == fechaFiscal.AddMinutes(1), "Fecha fiscal nullable conserva el instante");
        Check(await db.AuditoriaEventos.AnyAsync(e => e.Accion == "ABONO_VENTA_REGISTRADO"), "Auditoría persiste con la operación");
        async Task<RegistroCompras> CompraDistribuidora(string? nombre, int? id = null)
        {
            await using var c = new SistemaDbContext(options);
            return await new CrearCompraService(c, new MovimientoCajaService(c)).CrearCompra(new() {
                nombre_proveedor = nombre, id_proveedor = id,
                detalle_compra = [new() { id_producto = producto.IdProducto, cantidad = 1, precio = 5 }]
            }, admin.id_usuario);
        }
        var nuevaDistribuidora = await CompraDistribuidora("  Distribuidora nueva  ");
        var seleccionada = await CompraDistribuidora(null, nuevaDistribuidora.IdProveedor);
        var encontrada = await CompraDistribuidora(" distribuidora NUEVA ");
        Check(nuevaDistribuidora.IdProveedor == seleccionada.IdProveedor && encontrada.IdProveedor == seleccionada.IdProveedor &&
            await db.proveedores.CountAsync(p => p.id_proveedor == seleccionada.IdProveedor) == 1,
            "Compra crea o reutiliza distribuidora por ID o nombre sin duplicar");
        async Task<RegistroCompras> CompraConcurrente()
        {
            for (var attempt = 0; ; attempt++)
                try { return await CompraDistribuidora("Distribuidora simultánea"); }
                catch (Exception e) when (attempt < 3 && e.GetBaseException() is PostgresException {
                    SqlState: PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.DeadlockDetected }) { }
        }
        var simultaneas = await Task.WhenAll(CompraConcurrente(), CompraConcurrente());
        var mayusculas = await CompraDistribuidora("DISTRIBUIDORA SIMULTÁNEA");
        Check(simultaneas[0].IdProveedor == simultaneas[1].IdProveedor &&
            mayusculas.IdProveedor == simultaneas[0].IdProveedor &&
            await db.proveedores.CountAsync(p => p.nombre == "Distribuidora simultánea") == 1,
            "Dos compras simultáneas reutilizan una sola distribuidora después del reintento por conflicto");
        Console.WriteLine($"{count} comprobaciones de integración PostgreSQL real aprobadas.");
    }
}
