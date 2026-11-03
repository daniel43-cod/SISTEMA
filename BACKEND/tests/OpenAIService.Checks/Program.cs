using System.Net;
using System.Text.Json;
using API_SISTEMA.Services.IA;

var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
{
    ["OPENAI_API_KEY"] = "test-key", ["OPENAI_MODEL"] = "test-model"
}).Build();
var history = new[] { new TurnoIA("user", "¿Hay Coca-Cola?") };
const string success = """{"status":"completed","output":[{"type":"message","content":[{"type":"output_text","text":"Tenemos Coca-Cola."}]}]}""";
const string toolCall = """{"status":"completed","output":[{"type":"function_call","name":"BuscarProductos","call_id":"call_1","arguments":"{\"texto\":\"coca\"}"}]}""";
int passed = 0;
void Check(bool value) { if (!value) throw new Exception("Comprobación fallida"); }
HttpResponseMessage Json(string value) => new(HttpStatusCode.OK) { Content = new StringContent(value) };
Task<string> NoTools(string _, CancellationToken ct) => throw new Exception("No se esperaba una herramienta");
OpenAIService Create(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
    => new(new HttpClient(new FakeHandler(handler)), config);

var calls = 0;
var service = Create(async (request, ct) =>
{
    Check(request.RequestUri!.AbsoluteUri == "https://api.openai.com/v1/responses");
    Check(request.Headers.Authorization?.Parameter == "test-key");
    using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
    Check(!body.RootElement.GetProperty("store").GetBoolean());
    Check(body.RootElement.GetProperty("instructions").GetString()!.Contains("disponible=false"));
    if (++calls == 1) return Json(toolCall);
    Check(body.RootElement.GetProperty("input").EnumerateArray()
        .Any(x => x.TryGetProperty("type", out var t) && t.GetString() == "function_call_output" &&
            x.GetProperty("call_id").GetString() == "call_1"));
    return Json(success);
});
var searches = 0;
var result = await service.ResponderAsync(history, (text, ct) =>
{
    Check(text == "coca"); searches++;
    return Task.FromResult("{\"productos\":[{\"nombre\":\"Coca-Cola\"}]}");
});
Check(result == "Tenemos Coca-Cola." && searches == 1 && calls == 2); passed++;

var categoryCalls = 0;
var categorySearches = 0;
var categoryService = Create((request, ct) => Task.FromResult(++categoryCalls == 1
    ? Json(toolCall.Replace("BuscarProductos", "BuscarMarcasPorCategoria").Replace("coca", "golosinas"))
    : Json(success)));
await categoryService.ResponderAsync(history, NoTools, buscarMarcasPorCategoria: (text, ct) =>
{
    Check(text == "golosinas"); categorySearches++;
    return Task.FromResult("{\"categorias\":[{\"categoria\":\"Golosinas\",\"marcas\":[\"Marca A\"]}]}");
});
Check(categorySearches == 1 && categoryCalls == 2); passed++;

var zoneCalls = 0;
var zoneSearches = 0;
const string zoneCall = """{"status":"completed","output":[{"type":"function_call","name":"ConsultarZonasEntrega","call_id":"zone_1","arguments":"{\"texto\":\"\"}"}]}""";
var zoneService = Create(async (request, ct) =>
{
    using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
    Check(body.RootElement.GetProperty("tools").EnumerateArray()
        .Any(t => t.GetProperty("name").GetString() == "ConsultarZonasEntrega"));
    if (++zoneCalls == 1) return Json(zoneCall);
    Check(body.RootElement.GetProperty("input").EnumerateArray().Any(x =>
        x.TryGetProperty("call_id", out var id) && id.GetString() == "zone_1" &&
        x.TryGetProperty("output", out var output) && output.GetString()!.Contains("Purulhá")));
    return Json(success);
});
await zoneService.ResponderAsync(history, NoTools, consultarZonasEntrega: (text, ct) =>
{
    Check(text == ""); zoneSearches++;
    return Task.FromResult("""{"moneda":"GTQ","regla":"totalMayorAlMinimo","zonas":[{"zona":"Purulhá","minimoCompra":100}]}""");
});
Check(zoneSearches == 1 && zoneCalls == 2); passed++;
try
{
    await Create((r, ct) => Task.FromResult(Json(zoneCall))).ResponderAsync(history, NoTools);
    throw new Exception("Faltó error de herramienta no disponible");
}
catch (OpenAIException ex) { Check(ex.StatusCode == 503); passed++; }

foreach (var filtro in new[] { "", "bebidas" })
{
    var listCalls = 0;
    var listSearches = 0;
    var listCall = zoneCall.Replace("ConsultarZonasEntrega", "ConsultarCategorias")
        .Replace("zone_1", "category_list_1")
        .Replace("\\\"texto\\\":\\\"\\\"", "\\\"texto\\\":\\\"" + filtro + "\\\"");
    var listService = Create(async (request, ct) =>
    {
        using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
        Check(body.RootElement.GetProperty("tools").EnumerateArray()
            .Any(t => t.GetProperty("name").GetString() == "ConsultarCategorias"));
        if (++listCalls == 1) return Json(listCall);
        Check(body.RootElement.GetProperty("input").EnumerateArray().Any(x =>
            x.TryGetProperty("call_id", out var id) && id.GetString() == "category_list_1" &&
            x.TryGetProperty("output", out var output) && output.GetString()!.Contains("Bebidas")));
        return Json(success);
    });
    await listService.ResponderAsync(history, NoTools, consultarCategorias: (text, ct) =>
    {
        Check(text == filtro); listSearches++;
        return Task.FromResult("""{"categorias":[{"nombre":"Bebidas"}]}""");
    });
    Check(listCalls == 2 && listSearches == 1); passed++;
}
try
{
    await Create((r, ct) => Task.FromResult(Json(zoneCall.Replace("ConsultarZonasEntrega", "ConsultarCategorias"))))
        .ResponderAsync(history, NoTools);
    throw new Exception("Faltó error de lista de categorías no disponible");
}
catch (OpenAIException ex) { Check(ex.StatusCode == 503); passed++; }

var productCategoryCalls = 0;
var productCategorySearches = 0;
var productCategoryCall = toolCall.Replace("BuscarProductos", "BuscarProductosPorCategoria")
    .Replace("coca", "bebidas");
var productCategoryService = Create(async (request, ct) =>
{
    using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
    Check(body.RootElement.GetProperty("tools").EnumerateArray().Any(t =>
        t.GetProperty("name").GetString() == "BuscarProductosPorCategoria"));
    if (++productCategoryCalls == 1) return Json(productCategoryCall);
    Check(body.RootElement.GetProperty("input").EnumerateArray().Any(x =>
        x.TryGetProperty("type", out var type) && type.GetString() == "function_call_output" &&
        x.TryGetProperty("output", out var output) && output.GetString()!.Contains("Agua")));
    return Json(success);
});
await productCategoryService.ResponderAsync(history, NoTools,
    buscarMarcasPorCategoria: NoTools,
    buscarProductosPorCategoria: (text, ct) =>
    {
        Check(text == "bebidas"); productCategorySearches++;
        return Task.FromResult("""{"limite":20,"hayMas":false,"productos":[{"categoria":"Bebidas","nombre":"Agua","disponible":true}]}""");
    });
Check(productCategoryCalls == 2 && productCategorySearches == 1); passed++;
try
{
    await Create((r, ct) => Task.FromResult(Json(productCategoryCall))).ResponderAsync(history, NoTools);
    throw new Exception("Faltó error de consulta de productos por categoría no disponible");
}
catch (OpenAIException ex) { Check(ex.StatusCode == 503); passed++; }

async Task Error(OpenAIService sut, int expected)
{
    try { await sut.ResponderAsync(history, NoTools); throw new Exception("Faltó excepción"); }
    catch (OpenAIException ex) { Check(ex.StatusCode == expected); passed++; }
}
foreach (var (status, expected) in new[] { (429, 429), (401, 503), (500, 502) })
    await Error(Create((r, ct) => Task.FromResult(new HttpResponseMessage((HttpStatusCode)status))), expected);
await Error(Create((r, ct) => Task.FromResult(Json("invalid json"))), 502);
await Error(Create((r, ct) => Task.FromResult(Json("""{"status":"incomplete","output":[]} """))), 502);
await Error(Create((r, ct) => throw new HttpRequestException()), 502);
await Error(Create((r, ct) => throw new OperationCanceledException()), 504);
await Error(Create((r, ct) => Task.FromResult(Json(toolCall.Replace("BuscarProductos", "BorrarProductos")))), 502);
await Error(new OpenAIService(new HttpClient(new FakeHandler((r, ct) => throw new Exception("No debe llamar HTTP"))),
    new ConfigurationBuilder().Build()), 503);

using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
try
{
    await Create((r, ct) => { ct.ThrowIfCancellationRequested(); return Task.FromResult(Json(success)); })
        .ResponderAsync(history, NoTools, cancellation.Token);
    throw new Exception("Faltó cancelación");
}
catch (OperationCanceledException) { passed++; }
Console.WriteLine($"{passed} comprobaciones correctas; sin OpenAI ni SQL reales.");

sealed class FakeHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        => handler(request, ct);
}
