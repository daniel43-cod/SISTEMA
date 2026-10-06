using System.ComponentModel.DataAnnotations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace API_SISTEMA.services.ProductoS;

// Maneja archivos propios; nunca descarga las URLs externas.
public sealed class ProductoImagenService
{
    public const long MaxBytes = 5 * 1024 * 1024;
    private readonly string _carpeta;
    private readonly ILogger<ProductoImagenService> _logger;

    public ProductoImagenService(IWebHostEnvironment environment, ILogger<ProductoImagenService> logger)
    {
        _carpeta = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"),
            "uploads", "productos");
        _logger = logger;
    }

    public string? ValidarUrl(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return null;
        valor = valor.Trim();
        if (valor.Length > 2048 || !Uri.TryCreate(valor, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps || string.IsNullOrEmpty(uri.Host) || uri.UserInfo.Length > 0)
            throw new ValidationException("La imagen debe tener una URL HTTPS válida de hasta 2048 caracteres.");
        var urlNormalizada = uri.AbsoluteUri;
        if (urlNormalizada.Length > 2048)
            throw new ValidationException("La imagen debe tener una URL HTTPS válida de hasta 2048 caracteres.");
        return urlNormalizada;
    }

    public async Task<string> GuardarAsync(IFormFile archivo, CancellationToken cancellationToken)
    {
        if (archivo.Length <= 0 || archivo.Length > MaxBytes)
            throw new ValidationException("La imagen debe pesar entre 1 byte y 5 MB.");

        // Acotar la lectura aunque el tamaño declarado sea incorrecto.
        using var contenido = new MemoryStream();
        await using (var entrada = archivo.OpenReadStream())
        {
            var buffer = new byte[81920];
            int leidos;
            while ((leidos = await entrada.ReadAsync(buffer, cancellationToken)) > 0)
            {
                if (contenido.Length + leidos > MaxBytes)
                    throw new ValidationException("La imagen supera los 5 MB.");
                await contenido.WriteAsync(buffer.AsMemory(0, leidos), cancellationToken);
            }
        }
        contenido.Position = 0;
        var opciones = new DecoderOptions { MaxFrames = 1, SkipMetadata = false };
        try
        {
            var info = await Image.IdentifyAsync(opciones, contenido, cancellationToken);
            if (info.Width > 8000 || info.Height > 8000 || (long)info.Width * info.Height > 32_000_000)
                throw new ValidationException("La imagen admite hasta 8000 píxeles por lado y 32 megapíxeles.");
            var formato = info.Metadata.DecodedImageFormat?.Name;
            if (formato is not ("JPEG" or "PNG" or "WEBP"))
                throw new ValidationException("Selecciona una imagen JPEG, PNG o WebP.");

            contenido.Position = 0;
            using var imagen = await Image.LoadAsync(opciones, contenido, cancellationToken);
            imagen.Mutate(x => x.AutoOrient().Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max, Size = new Size(Math.Min(1200, imagen.Width), Math.Min(1200, imagen.Height))
            }));
            // No publicar metadatos de cámara, ubicación o contenido añadido al archivo.
            imagen.Metadata.ExifProfile = null;
            imagen.Metadata.XmpProfile = null;
            imagen.Metadata.IccProfile = null;
            imagen.Metadata.IptcProfile = null;
            Directory.CreateDirectory(_carpeta);
            var nombre = Guid.NewGuid().ToString("N") + ".webp";
            var ruta = Path.Combine(_carpeta, nombre);
            try { await imagen.SaveAsWebpAsync(ruta, cancellationToken); }
            catch { Eliminar("/uploads/productos/" + nombre); throw; }
            return "/uploads/productos/" + nombre;
        }
        catch (UnknownImageFormatException)
        {
            throw new ValidationException("El archivo no es una imagen compatible.");
        }
        catch (InvalidImageContentException)
        {
            throw new ValidationException("La imagen está dañada o no es válida.");
        }
    }

    // Solo elimina nombres generados por este servicio.
    public void Eliminar(string ruta)
    {
        var nombre = Path.GetFileName(ruta);
        if (ruta != "/uploads/productos/" + nombre ||
            Path.GetExtension(nombre) != ".webp" ||
            !Guid.TryParseExact(Path.GetFileNameWithoutExtension(nombre), "N", out _)) return;
        try { File.Delete(Path.Combine(_carpeta, nombre)); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        { _logger.LogWarning(error, "No se pudo limpiar una imagen de producto sin guardar."); }
    }
}


