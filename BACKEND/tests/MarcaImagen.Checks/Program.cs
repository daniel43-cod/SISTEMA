using API_SISTEMA.Services.Marca;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

var root = Path.Combine(Path.GetTempPath(), "marca-check-" + Guid.NewGuid().ToString("N"));
var service = new MarcaImagenService(new TestEnvironment { WebRootPath = root }, NullLogger<MarcaImagenService>.Instance);
if (service.ValidarUrl(null) != null || service.ValidarUrl(" https://example.com/a.jpg ") != "https://example.com/a.jpg") throw new Exception("URL");
foreach (var url in new[] { "http://example.com/a", "file:///a", "/uploads/a", "https://user:password@example.com/a" })
{
    try { service.ValidarUrl(url); throw new Exception("URL aceptada"); }
    catch (ValidationException) { }
}
foreach (var bytes in new[] { "<svg></svg>"u8.ToArray(), new byte[0], new byte[5 * 1024 * 1024 + 1] })
{
    using var stream = new MemoryStream(bytes);
    try { await service.GuardarAsync(new FormFile(stream, 0, stream.Length, "Imagen", "foto.jpg"), default); throw new Exception("Archivo aceptado"); }
    catch (ValidationException) { }
}
using var image = new Image<Rgba32>(20, 20);
using var png = new MemoryStream();
await image.SaveAsPngAsync(png);
png.Position = 0;
var path = await service.GuardarAsync(new FormFile(png, 0, png.Length, "Imagen", "../../foto.png"), default);
var saved = Path.Combine(root, "uploads", "marcas", Path.GetFileName(path));
using (var decoded = Image.Load(saved))
    if (decoded.Width != 20 || !path.EndsWith(".webp")) throw new Exception("Conversión");
service.Eliminar(path);
if (File.Exists(saved)) throw new Exception("Limpieza");
Console.WriteLine("OK: URLs, archivos falsos/vacíos/excesivos, conversión real, nombre seguro y limpieza.");

sealed class TestEnvironment : IWebHostEnvironment
{
    public string WebRootPath { get; set; } = "";
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    public string ApplicationName { get; set; } = "Checks";
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    public string ContentRootPath { get; set; } = "";
    public string EnvironmentName { get; set; } = "Development";
}

