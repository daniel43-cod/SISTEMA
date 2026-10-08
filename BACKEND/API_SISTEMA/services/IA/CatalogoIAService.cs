using System.Text.Json;
using API_SISTEMA.Data;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.IA;

// sealed impide heredar de esta clase. El constructor primario recibe el contexto
public sealed class CatalogoIAService(SistemaDbContext context)
{
    // Busca por texto y devuelve JSON. async/Task<string> permiten esperar la consulta
    // sin bloquear el hilo. ct permite solicitar la cancelación de la operación SQL.
    public async Task<string> BuscarProductos(string texto, CancellationToken ct)
    {
        var productos = await context.producto_presentaciones.AsNoTracking()
            // p representa una presentación. Exige que esté activa y que el texto
            // aparezca en el nombre del producto o en la descripción de la presentación.
            // && significa «y»; || significa «o». EF traduce este filtro a SQL.
            .Where(p => p.estado && (p.Producto.nombre.Contains(texto) || (p.Presentacion.Descripcion != null && p.Presentacion.Descripcion.Contains(texto))))
            // Ordena por nombre; si hay nombres iguales, ordena por ID de presentación.
            .OrderBy(p => p.Producto.nombre).ThenBy(p => p.id_producto_presentacion)
            // Limita a diez resultados en SQL: no carga todo el catálogo en memoria.
            .Take(10)
            // Selecciona únicamente los datos que la IA necesita, en un objeto anónimo.
            .Select(p => new
            {
                // nombre: nombre del producto; presentacion: descripción de su formato;
                // precio: precio registrado para esa presentación, no el costo de compra.
                nombre = p.Producto.nombre, presentacion = p.Presentacion.Descripcion, precio = p.precio,
                // Comprueba que la presentación tenga una equivalencia positiva y haya stock.
                // ?? 0 trata el stock nulo como cero y evita dividir entre cero más abajo.
                presentacionesDisponibles = p.unidades_equivalentes > 0 && (p.Producto.stock ?? 0) > 0
                    // Operador ternario: si cumple la condición, divide stock entre unidades
                    // por presentación; si no, devuelve cero. La división es entera:
                    // 25 unidades / 12 por caja = 2 cajas completas disponibles.
                    ? (p.Producto.stock ?? 0) / p.unidades_equivalentes : 0
            // Aquí se ejecuta la consulta y se materializan hasta diez resultados en memoria.
            }).ToListAsync(ct);
        // Convierte los resultados a JSON para que OpenAIService los envíe al modelo.
        // limite indica el máximo de resultados, no el total de productos existentes.
        // Si no hay coincidencias, productos será un arreglo vacío.
        return JsonSerializer.Serialize(new { limite = 10, productos });
    }
}
