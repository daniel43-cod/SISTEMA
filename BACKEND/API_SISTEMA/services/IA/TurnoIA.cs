// Importa los tipos para construir encabezados HTTP, como Authorization.
using System.Net.Http.Headers;
// Permite convertir objetos de C# en contenido JSON para enviarlos por HTTP.
using System.Net.Http.Json;
// Permite leer el JSON recibido de OpenAI con JsonDocument y JsonElement.
using System.Text.Json;
// Permite construir y ampliar el JSON del historial con JsonArray y JsonObject.
using System.Text.Json.Nodes;

// Agrupa estos tipos dentro del espacio de nombres de los servicios de IA.
namespace API_SISTEMA.Services.IA;

public record TurnoIA(string Rol, string Contenido);
