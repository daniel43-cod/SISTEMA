using API_SISTEMA.Models;
using API_SISTEMA.Services.Comprobantes;
using SixLabors.ImageSharp;

var empresa = new Empresa { nombre_empresa = "Distribuidora San Antonio", ubicacion = "Purulha B.V", direccion = "Barrio San Antonio" };
var venta = new Ventas { id_ventas = 123, origen = "MOSTRADOR", fecha_venta = new DateTime(2026, 11, 9, 14, 30, 0),
    cliente = new Cliente { nombre = "Cliente de ejemplo" }, subtotal = 100, descuento = 5,
    total = 95, monto_pagado = 60, saldo_pendiente = 35 };
var salida = Path.GetFullPath(args.FirstOrDefault() ?? "artifacts/comprobantes");
Directory.CreateDirectory(salida);
foreach (var cantidad in new[] { 0, 2, 100 })
{
    venta.DetalleVentas = Enumerable.Range(1, cantidad).Select(i => new DetalleVenta {
        id_detalle_venta = i, cantidad = 2, precio = 25, descuento = 2.5m, subtotal = 47.5m,
        Producto = new Productos { Nombre = i % 2 == 0 ? "Producto con descripción larga para comprobar el ajuste automático de texto" : "Agua pura" },
        producto_presentacion = new ProductoPresentacion { Presentacion = new Presentacion { Descripcion = "Caja de 24 unidades" } }
    }).ToList();
    venta.subtotal = cantidad * 50; venta.descuento = cantidad * 2.5m; venta.total = venta.subtotal - venta.descuento.Value;
    venta.monto_pagado = venta.total / 2; venta.saldo_pendiente = venta.total - venta.monto_pagado;
    var paginas = ComprobanteImagen.Planificar(venta, empresa);
    if (paginas.Sum(p => p.Filas.Count) != cantidad) throw new Exception("Se perdieron detalles.");
    if (cantidad == 100 && paginas.Count < 2) throw new Exception("No se paginó la venta extensa.");
    for (int p = 1; p <= paginas.Count; p++)
    {
        var png = ComprobanteImagen.Generar(venta, empresa, paginas, p);
        using var imagen = Image.Load(png);
        if (imagen.Width != 1200 || imagen.Height > 2400) throw new Exception("Dimensiones incorrectas.");
        if (png.Length > 5 * 1024 * 1024) throw new Exception("Imagen mayor de 5 MB.");
        File.WriteAllBytes(Path.Combine(salida, $"venta-{cantidad}-{p}.png"), png);
    }
    Console.WriteLine($"OK: {cantidad} detalles, {paginas.Count} página(s), sin pérdida de filas.");
}
