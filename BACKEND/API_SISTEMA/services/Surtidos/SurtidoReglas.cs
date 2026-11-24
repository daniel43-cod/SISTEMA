using System.ComponentModel.DataAnnotations;
using Producto = API_SISTEMA.Models.Productos;

namespace API_SISTEMA.Services.Surtidos;

public static class SurtidoReglas
{
    public static decimal PrecioMinimoPresentacion(IEnumerable<Producto> productos, int unidades)
    {
        var lista = productos.ToArray();
        ValidarCostosPromedio(lista);
        if (unidades <= 0)
            throw new ValidationException("Las unidades equivalentes deben ser mayores a cero.");
        // Redondear hacia arriba permite expresar el mínimo en centavos sin vender bajo costo.
        var costoMaximo = lista.Max(p => p.PrecioPromedioCompra!.Value) * unidades;
        return decimal.Ceiling(costoMaximo * 100m) / 100m;
    }

    public static void ValidarPrecioPresentacion(IEnumerable<Producto> productos, int unidades, decimal precio)
    {
        API_SISTEMA.Services.Ventas.VentaReglas.Monto(precio, false);
        var minimo = PrecioMinimoPresentacion(productos, unidades);
        if (precio < minimo)
            throw new ValidationException($"El precio de la presentación debe ser al menos Q{minimo:0.00}, para cubrir la combinación más cara.");
    }

    // El importe ya debe incluir el descuento. El costo es la suma real de los componentes,
    // conservando sus decimales; la parte entera solo se usa para comprobar compatibilidad.
    public static void ValidarImporteVenta(decimal importeNeto, decimal costoTotalComponentes)
    {
        if (costoTotalComponentes <= 0)
            throw new ValidationException("El costo total de los componentes del surtido debe ser mayor a cero.");

        if (importeNeto < costoTotalComponentes)
            throw new ValidationException("El importe del surtido después del descuento no puede ser menor al costo de sus componentes.");
    }

    // Compara el costo promedio por unidad base. No redondea ni modifica los costos.
    public static void ValidarCostosPromedio(IEnumerable<Producto> productos)
    {
        ArgumentNullException.ThrowIfNull(productos);
        decimal? parteEntera = null;

        foreach (var producto in productos)
        {
            if (producto is null || producto.PrecioPromedioCompra is not decimal costo || costo <= 0)
                throw new ValidationException("Los productos del surtido deben tener un costo promedio mayor a cero.");

            var enteroActual = decimal.Truncate(costo);
            if (parteEntera.HasValue && parteEntera.Value != enteroActual)
                throw new ValidationException("Hay mucha diferencia entre los precios de los producto seleccionados.");

            parteEntera = enteroActual;
        }

        if (!parteEntera.HasValue)
            throw new ValidationException("No hay productos para comprobar los costos promedio del surtido.");
    }
}
