using System.Net;
using System.Text.Json;
using API_SISTEMA.services.IA;

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
