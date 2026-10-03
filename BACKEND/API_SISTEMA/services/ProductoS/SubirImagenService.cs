using API_SISTEMA.data;
using API_SISTEMA.models;

namespace API_SISTEMA.services.ProductoS;

// Reutiliza la misma validación de imágenes también al reemplazar una imagen existente.
public class SubirImagenService
{
    private readonly SistemaDbContext _context;
    private readonly ProductoImagenService _imagenes;
    public SubirImagenService(SistemaDbContext context, ProductoImagenService imagenes)
    {
        _context = context;
        _imagenes = imagenes;
    }
    public async Task<Productos?> SubirImagen(int id, IFormFile imagen)
    {
        var producto = await _context.productos.FindAsync(id);
        if (producto is null) return null;
        var ruta = await _imagenes.GuardarAsync(imagen, default);
        producto.imagen = ruta;
        // Ante un resultado incierto de SQL conservar el archivo evita referencias rotas.
        await _context.SaveChangesAsync();
        return producto;
    }
}
