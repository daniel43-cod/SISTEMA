using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Numerics;
using API_SISTEMA.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace API_SISTEMA.Services.Comprobantes;

/// <summary>Renderizador sin acceso a base de datos ni envío de mensajes.</summary>
public static class ComprobanteImagen
{
    public const int Ancho = 1200;
    private const int AltoMaximo = 2400;
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-GT");
    private const int EspacioTotales = 350;
    private static readonly int[] X = [40, 120, 360, 950];
    private static readonly int[] W = [70, 220, 570, 210];
    public sealed record Fila(string[] Celdas, int Alto, string? Descuento = null);
    public sealed record Pagina(List<Fila> Filas, int Encabezado, int Alto);

    private static Font Fuente(float tamano = 26, bool negrita = false)
    {
        foreach (var nombre in new[] { "Arial", "DejaVu Sans", "Liberation Sans" })
            if (SystemFonts.TryGet(nombre, out var familia))
                return familia.CreateFont(tamano, negrita ? FontStyle.Bold : FontStyle.Regular);
        throw new InvalidOperationException("Instala Arial, DejaVu Sans o Liberation Sans para generar comprobantes.");
    }

    private static string Moneda(decimal valor) => "Q" + valor.ToString("N2", Cultura);
    private static int Medir(string texto, Font fuente, int ancho) =>
        (int)Math.Ceiling(TextMeasurer.MeasureAdvance(texto, new TextOptions(fuente) { WrappingLength = ancho }).Height) + 12;

    private static List<(string Texto, int Tamano, bool Negrita, bool Centrado)> Cabecera(Models.Ventas venta, Empresa empresa, bool pagos = false) =>
        new[] {
            (empresa.nombre_empresa, 38, true, true), (empresa.ubicacion ?? "", 28, false, true),
            (empresa.direccion ?? "", 28, false, true), (empresa.telefonos ?? "", 26, false, true),
            ($"{(pagos ? "Historial de pagos - Venta" : "Comprobante de venta")} #{venta.id_ventas}", 30, true, false),
            ($"Fecha: {venta.fecha_venta:dd/MM/yyyy HH:mm}", 26, false, false),
            ($"Cliente: {venta.cliente.nombre} {venta.cliente.apellido}".Trim(), 26, false, false)
        }.Where(x => !string.IsNullOrWhiteSpace(x.Item1)).ToList();

    public static List<Pagina> Planificar(Models.Ventas venta, Empresa empresa)
    {
        var fuente = Fuente();
        int encabezado = 50 + Cabecera(venta, empresa).Sum(c => Medir(c.Texto, Fuente(c.Tamano, c.Negrita), Ancho - 80) + 8) + 75;
        // Reservar el espacio de totales en todas las páginas permite alturas acotadas.
        int capacidad = AltoMaximo - encabezado - EspacioTotales;
        if (capacidad < 150) throw new ValidationException("Los datos del encabezado son demasiado extensos.");
        var grupos = new List<List<Fila>> { new() };
        int usado = 0;
        foreach (var detalle in venta.DetalleVentas.OrderBy(d => d.id_detalle_venta))
        {
            string[] celdas = [detalle.cantidad.ToString(Cultura),
                detalle.producto_presentacion?.Presentacion?.Descripcion ?? "Sin presentación", detalle.Surtido?.Nombre ?? detalle.Producto?.Nombre ?? "Producto",
                Moneda(detalle.cantidad * detalle.precio)];
            string? descuento = detalle.descuento > 0 ? "−" + Moneda(detalle.descuento.Value) : null;
            int altoPrecio = Medir(celdas[^1], fuente, W[^1]) +
                (descuento is null ? 0 : Medir(descuento, fuente, W[^1]));
            int alto = Math.Max(60, Math.Max(celdas.Select((c, i) => Medir(c, fuente, W[i])).Max(), altoPrecio) + 20);
            if (alto > capacidad) throw new ValidationException("Un detalle es demasiado extenso para una imagen legible.");
            if (usado + alto > capacidad) { grupos.Add(new()); usado = 0; }
            grupos[^1].Add(new(celdas, alto, descuento));
            usado += alto;
        }
        return grupos.Select((filas, i) => new Pagina(filas, encabezado,
            encabezado + filas.Sum(f => f.Alto) + (i == grupos.Count - 1 ? EspacioTotales : 100))).ToList();
    }

    public static List<Pagina> PlanificarPagos(Models.Ventas venta, Empresa empresa,
        IEnumerable<API_SISTEMA.Dtos.Pagos.PagoVentaHistorialDto> pagos)
    {
        int encabezado = 50 + Cabecera(venta, empresa, true).Sum(c => Medir(c.Texto, Fuente(c.Tamano, c.Negrita), Ancho - 80) + 8) + 75;
        int capacidad = AltoMaximo - encabezado - EspacioTotales;
        if (capacidad < 150) throw new ValidationException("Los datos del encabezado son demasiado extensos.");
        var grupos = new List<List<Fila>> { new() };
        int usado = 0;
        foreach (var pago in pagos.OrderBy(p => p.FechaPago).ThenBy(p => p.IdPago))
        {
            string[] celdas = [pago.Usuario, pago.FechaPago.ToString("dd/MM/yyyy HH:mm", Cultura), Moneda(pago.Monto)];
            int[] anchos = [500, 350, 210];
            int alto = Math.Max(60, celdas.Select((c, i) => Medir(c, Fuente(), anchos[i])).Max() + 20);
            if (alto > capacidad) throw new ValidationException("Un pago es demasiado extenso para una imagen legible.");
            if (usado + alto > capacidad) { grupos.Add(new()); usado = 0; }
            grupos[^1].Add(new(celdas, alto)); usado += alto;
        }
        return grupos.Select((filas, i) => new Pagina(filas, encabezado,
            encabezado + filas.Sum(f => f.Alto) + (i == grupos.Count - 1 ? EspacioTotales : 100))).ToList();
    }

    public static byte[] Generar(Models.Ventas venta, Empresa empresa, IReadOnlyList<Pagina> paginas, int numero, bool historialPagos = false)
    {
        var pagina = paginas[numero - 1];
        using var imagen = new Image<Rgb24>(Ancho, pagina.Alto, new Rgb24(255, 255, 255));
        imagen.Mutate(ctx => ctx.Paint(canvas =>
        {
            void Texto(string texto, int x, int y, int ancho, int tamano = 26, bool negrita = false, bool centrado = false, Color? color = null)
            {
                canvas.DrawText(new RichTextOptions(Fuente(tamano, negrita))
                { Origin = new Vector2(centrado ? x + ancho / 2f : x, y), WrappingLength = ancho,
                    HorizontalAlignment = centrado ? HorizontalAlignment.Center : HorizontalAlignment.Left,
                    TextAlignment = centrado ? TextAlignment.Center : TextAlignment.Start }, texto, Brushes.Solid(color ?? Color.Black), null);
            }
            int y = 40;
            foreach (var c in Cabecera(venta, empresa, historialPagos))
            {
                Texto(c.Texto, 40, y, Ancho - 80, c.Tamano, c.Negrita, c.Centrado);
                y += Medir(c.Texto, Fuente(c.Tamano, c.Negrita), Ancho - 80) + 8;
            }
            int[] columnasX = historialPagos ? [40, 570, 950] : X;
            int[] columnasW = historialPagos ? [500, 350, 210] : W;
            string[] titulos = historialPagos ? ["Encargado", "Fecha", "Monto"] : ["Cant.", "Presentación", "Producto", "Subtotal"];
            y += 10;
            for (int i = 0; i < titulos.Length; i++) Texto(titulos[i], columnasX[i], y, columnasW[i], 24, true);
            y = pagina.Encabezado;
            foreach (var fila in pagina.Filas)
            {
                for (int i = 0; i < fila.Celdas.Length; i++) Texto(fila.Celdas[i], columnasX[i], y, columnasW[i]);
                if (fila.Descuento is not null)
                    Texto(fila.Descuento, X[^1], y + Medir(fila.Celdas[^1], Fuente(), W[^1]), W[^1],
                        color: Color.FromPixel(new Rgb24(22, 130, 70)));
                y += fila.Alto;
            }
            if (numero == paginas.Count)
            {
                y += 20;
                (string Etiqueta, decimal Valor, Color Color)[] totales = [ ("Subtotal", venta.subtotal, Color.Black),
                    ("Total", venta.total, Color.Black), ("Monto pagado", venta.monto_pagado, Color.FromPixel(new Rgb24(22, 130, 70))),
                    ("Saldo pendiente", venta.saldo_pendiente, Color.FromPixel(new Rgb24(210, 106, 0))) ];
                foreach (var total in totales.Where(t => !historialPagos || (t.Etiqueta == "Saldo pendiente" && t.Valor > 0)))
                {
                    Texto(total.Etiqueta, 510, y, 400, 28, true, color: total.Color);
                    Texto(Moneda(total.Valor), 950, y, 210, 28, true, color: total.Color);
                    y += 48;
                }
                Texto("¡Gracias por tu compra!", 40, y + 12, Ancho - 80, 28, centrado: true);
            }
            Texto($"Página {numero} de {paginas.Count}", 40, pagina.Alto - 50, 500, 22);
        }));
        using var salida = new MemoryStream();
        imagen.SaveAsPng(salida);
        return salida.ToArray();
    }
}
