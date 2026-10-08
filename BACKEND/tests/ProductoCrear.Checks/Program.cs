using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using API_SISTEMA.Dtos.Productos;
using API_SISTEMA.Services.Productos;

CrearProductoDto Valid() => new()
{
    codigo_barra = "00123", nombre = "Coca 2.5", IdMarca = 1, stock_minimo = 0,
    presentaciones = [new() { id_presentacion = 1, unidades_equivalentes = 6, precio = 20.50m }]
};
void Invalid(Action<CrearProductoDto> change)
{
    var dto = Valid(); change(dto);
    try { ProductoCrearService.Validar(dto); }
    catch (ValidationException) { return; }
    throw new Exception("Se aceptaron datos inválidos.");
}
ProductoCrearService.Validar(Valid());
Invalid(d => d.nombre = " ");
Invalid(d => d.codigo_barra = "12 34");
Invalid(d => d.codigo_barra = new string('a', 101));
Invalid(d => d.IdMarca = 0);
Invalid(d => d.stock_minimo = -1);
Invalid(d => d.presentaciones = null!);
Invalid(d => d.presentaciones.Clear());
Invalid(d => d.presentaciones.Add(null!));
Invalid(d => d.presentaciones.Add(d.presentaciones[0]));
Invalid(d => d.presentaciones[0].unidades_equivalentes = 0);
Invalid(d => d.presentaciones[0].precio = -1);
Invalid(d => d.presentaciones[0].precio = 1.001m);
Invalid(d => d.presentaciones[0].precio = decimal.MaxValue);
foreach (var json in new[] {
    """{"unidades_equivalentes":"abc"}""",
    """{"unidades_equivalentes":0.5}""",
    """{"precio":"abc"}""",
    """{"unidades_equivalentes":2147483648}"""
})
{
    try { JsonSerializer.Deserialize<ProductoPresentacionDto>(json); }
    catch (JsonException) { continue; }
    throw new Exception("Se aceptó un tipo numérico inválido.");
}
Console.WriteLine("OK: validación de producto, colecciones, duplicados, rangos, decimales y tipos JSON.");
