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
    private static readonly int[] X = [40, 120, 510, 750, 950];
    private static readonly int[] W = [70, 370, 220, 180, 210];
    public sealed record Fila(string[] Celdas, int Alto);
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

    private static List<(string Texto, int Tamano, bool Negrita)> Cabecera(Models.Ventas venta, Empresa empresa) =>
        new[] {
            (empresa.nombre_empresa, 38, true), (empresa.ubicacion ?? "", 28, false),
            (empresa.direccion ?? "", 28, false), (empresa.telefonos ?? "", 26, false),
            ($"Comprobante de venta #{venta.id_ventas}", 30, true),
            ($"Fecha: {venta.fecha_venta:dd/MM/yyyy HH:mm}", 26, false),
            ($"Cliente: {venta.cliente.nombre} {venta.cliente.apellido}".Trim(), 26, false)
        }.Where(x => !string.IsNullOrWhiteSpace(x.Item1)).ToList();

    public static List<Pagina> Planificar(Models.Ventas venta, Empresa empresa)
    {
        var fuente = Fuente();
        int encabezado = 50 + Cabecera(venta, empresa).Sum(c => Medir(c.Texto, Fuente(c.Tamano, c.Negrita), Ancho - 80) + 8) + 75;
        // Reservar el espacio de totales en todas las páginas permite alturas acotadas.
        int capacidad = AltoMaximo - encabezado - 450;
        if (capacidad < 150) throw new ValidationException("Los datos del encabezado son demasiado extensos.");
        var grupos = new List<List<Fila>> { new() };
        int usado = 0;
        foreach (var detalle in venta.DetalleVentas.OrderBy(d => d.id_detalle_venta))
        {
            string[] celdas = [detalle.cantidad.ToString(Cultura), detalle.Producto.Nombre,
                detalle.producto_presentacion?.Presentacion?.Descripcion ?? "Sin presentación",
                Moneda(detalle.descuento ?? 0), Moneda(detalle.precio)];
            int alto = Math.Max(60, celdas.Select((c, i) => Medir(c, fuente, W[i])).Max() + 20);
            if (alto > capacidad) throw new ValidationException("Un detalle es demasiado extenso para una imagen legible.");
            if (usado + alto > capacidad) { grupos.Add(new()); usado = 0; }
            grupos[^1].Add(new(celdas, alto));
            usado += alto;
        }
        return grupos.Select((filas, i) => new Pagina(filas, encabezado,
            encabezado + filas.Sum(f => f.Alto) + (i == grupos.Count - 1 ? 450 : 100))).ToList();
    }

    public static byte[] Generar(Models.Ventas venta, Empresa empresa, IReadOnlyList<Pagina> paginas, int numero)
    {
        var pagina = paginas[numero - 1];
        using var imagen = new Image<Rgb24>(Ancho, pagina.Alto, new Rgb24(255, 255, 255));
        imagen.Mutate(ctx => ctx.Paint(canvas =>
        {
            void Texto(string texto, int x, int y, int ancho, int tamano = 26, bool negrita = false)
            {
                canvas.DrawText(new RichTextOptions(Fuente(tamano, negrita))
                { Origin = new Vector2(x, y), WrappingLength = ancho }, texto, Brushes.Solid(Color.Black), null);
            }
            int y = 40;
            foreach (var c in Cabecera(venta, empresa))
            {
                Texto(c.Texto, 40, y, Ancho - 80, c.Tamano, c.Negrita);
                y += Medir(c.Texto, Fuente(c.Tamano, c.Negrita), Ancho - 80) + 8;
            }
            string[] titulos = ["Cant.", "Producto", "Presentación", "Descuento", "Precio unit."];
            y += 10;
            for (int i = 0; i < titulos.Length; i++) Texto(titulos[i], X[i], y, W[i], 24, true);
            y = pagina.Encabezado;
            foreach (var fila in pagina.Filas)
            {
                for (int i = 0; i < fila.Celdas.Length; i++) Texto(fila.Celdas[i], X[i], y, W[i]);
                y += fila.Alto;
            }
            if (numero == paginas.Count)
            {
                y += 20;
                (string Etiqueta, decimal Valor)[] totales = [ ("Subtotal", venta.subtotal),
                    ("Descuento", venta.descuento ?? 0), ("Impuesto", venta.impuesto),
                    ("Total", venta.total), ("Monto pagado", venta.monto_pagado), ("Saldo pendiente", venta.saldo_pendiente) ];
                foreach (var total in totales)
                {
                    Texto(total.Etiqueta, 510, y, 400, 28, true);
                    Texto(Moneda(total.Valor), 950, y, 210, 28, true);
                    y += 48;
                }
                Texto("¡Gracias por tu compra!", 40, y + 12, 700, 28);
            }
            Texto($"Página {numero} de {paginas.Count}", 40, pagina.Alto - 50, 500, 22);
        }));
        using var salida = new MemoryStream();
        imagen.SaveAsPng(salida);
        return salida.ToArray();
    }
}
