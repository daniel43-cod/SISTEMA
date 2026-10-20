using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Models;

namespace API_SISTEMA.Services.Ventas;

public static class VentaReglas
{
    public const decimal MontoMaximo = 99999999.99m;
    public static void Monto(decimal value, bool cero = true)
    {
        if (value < (cero ? 0m : 0.01m) || value > MontoMaximo || decimal.Round(value, 2) != value)
            throw new ValidationException("Los importes deben ser positivos, con hasta dos decimales y máximo Q99,999,999.99.");
    }
    public static void Texto(string? value, int maximo, string campo, bool obligatorio = false)
    {
        if ((obligatorio && string.IsNullOrWhiteSpace(value)) || value?.Length > maximo || value?.Any(char.IsControl) == true)
            throw new ValidationException($"Revisa {campo}; admite hasta {maximo} caracteres sin caracteres de control.");
    }
    public static DetalleVenta Calcular(API_SISTEMA.Models.Productos producto, ProductoPresentacion presentacion, int cantidad, decimal descuento)
    {
        if (cantidad <= 0 || presentacion.unidades_equivalentes <= 0)
            throw new ValidationException("La cantidad y las unidades equivalentes deben ser mayores a cero.");
        Monto(descuento);
        Monto(presentacion.precio, false);
        if (producto.PrecioPromedioCompra is not decimal costo || costo <= 0)
            throw new ValidationException($"{producto.Nombre} no tiene un costo promedio válido; registra su compra antes de vender.");
        int unidades;
        try { unidades = checked(cantidad * presentacion.unidades_equivalentes); }
        catch (OverflowException) { throw new ValidationException("La cantidad de unidades excede el límite permitido."); }
        if (producto.Stock is not int stock || stock < unidades)
            throw new ValidationException($"Existencias insuficientes para {producto.Nombre}.");
        var bruto = cantidad * presentacion.precio;
        Monto(bruto);
        var neto = bruto - descuento;
        if (neto <= unidades * costo)
            throw new ValidationException($"El precio final de {producto.Nombre} debe ser mayor al costo promedio de las unidades vendidas.");
        Monto(neto, false);
        producto.Stock = stock - unidades;
        return new DetalleVenta
        {
            id_producto = producto.IdProducto, id_producto_presentacion = presentacion.id_producto_presentacion,
            cantidad = cantidad, precio = presentacion.precio, unidades_descontadas = unidades,
            costo_unitario_venta = costo, descuento = descuento, subtotal = neto, ganancia = neto - unidades * costo
        };
    }
}
