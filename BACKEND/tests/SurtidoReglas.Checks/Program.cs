using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Surtidos;

var passed = 0;
Productos[] ProductosConCostos(params decimal?[] costos) => costos.Select((costo, i) =>
    new Productos { IdProducto = i + 1, Nombre = "Producto de prueba", PrecioPromedioCompra = costo }).ToArray();

void Acepta(string nombre, params decimal?[] costos)
{
    var productos = ProductosConCostos(costos);
    SurtidoReglas.ValidarCostosPromedio(productos);
    if (!productos.Select(p => p.PrecioPromedioCompra).SequenceEqual(costos))
        throw new Exception("La comparación modificó costos: " + nombre);
    passed++;
    Console.WriteLine("OK " + nombre);
}

void Rechaza(string nombre, params decimal?[] costos)
{
    try { SurtidoReglas.ValidarCostosPromedio(ProductosConCostos(costos)); }
    catch (ValidationException) { passed++; Console.WriteLine("OK " + nombre); return; }
    throw new Exception("Se aceptaron costos incompatibles: " + nombre);
}

Acepta("Ejemplo permitido: 2.50, 2.33 y 2.70", 2.50m, 2.33m, 2.70m);
Rechaza("Ejemplo rechazado: 2.60, 3.60 y 1.50", 2.60m, 3.60m, 1.50m);
Acepta("Mismo entero con seis decimales", 2.000001m, 2.999999m);
Acepta("No redondea 2.999999 a 3", 2.999999m, 2.01m);
Rechaza("Cambiar de entero rechaza incluso por un centavo", 2.99m, 3.00m);
Rechaza("No basta que la diferencia sea menor a uno", 2.90m, 3.10m);
Acepta("Costos positivos menores a uno", 0.10m, 0.99m);
Rechaza("Costo promedio ausente", 2.50m, null);
Rechaza("Costo cero inválido", 0m, 0.20m);
Rechaza("Costo negativo inválido", -2.50m, -2.33m);
Rechaza("Sin productos no se pueden validar costos");
Acepta("Un costo válido puede comprobarse sin exigir un mínimo nuevo de productos", 2.50m);
Acepta("No convierte la parte entera a int", 999999999999.10m, 999999999999.90m);
Rechaza("Los enteros altos también deben coincidir", 999999999999.90m, 999999999998.90m);

var cambiantes = ProductosConCostos(2.33m, 2.70m);
SurtidoReglas.ValidarCostosPromedio(cambiantes);
cambiantes[1].PrecioPromedioCompra = 3.01m;
try { SurtidoReglas.ValidarCostosPromedio(cambiantes); throw new Exception("No detectó el cambio del costo actual."); }
catch (ValidationException) { passed++; Console.WriteLine("OK Revalidar detecta cambios del costo promedio"); }

void Importe(string nombre, decimal importeNeto, decimal costo, bool permitido)
{
    var rechazado = false;
    try { SurtidoReglas.ValidarImporteVenta(importeNeto, costo); }
    catch (ValidationException) { rechazado = true; }
    if (rechazado == permitido) throw new Exception("Resultado incorrecto: " + nombre);
    passed++;
    Console.WriteLine("OK " + nombre);
}

Importe("Permite vender exactamente al costo", 15m, 15m, true);
Importe("Permite vender con ganancia", 16m, 15m, true);
Importe("Rechaza venta por debajo del costo", 14m, 15m, false);
Importe("Descuento puede dejar el importe exactamente al costo", 20m - 5m, 15m, true);
Importe("Descuento no puede producir pérdida", 20m - 6m, 15m, false);
Importe("No redondea el costo hacia abajo para autorizar una pérdida", 15m, 15.000001m, false);
Importe("Compara el costo real, no su parte entera", 15m, 15.06m, false);
Importe("Costo real de seis componentes con promedios diferentes", 15.06m, 2m * 2.33m + 2m * 2.50m + 2m * 2.70m, true);
Importe("Costo cero inválido", 15m, 0m, false);
Importe("Costo negativo inválido", 15m, -1m, false);

Console.WriteLine($"{passed} comprobaciones aprobadas; sin acceder a la base de datos.");
