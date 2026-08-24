using API_SISTEMA.data;
using API_SISTEMA.models;

namespace API_SISTEMA.services.ProductoS
{
    public class SubirImagenService
    {
        private readonly ProductoCrearService _crearService;
        private readonly SistemaDbContext _context;
        public SubirImagenService(SistemaDbContext context, ProductoCrearService crearService)
        {
            _context = context;
            _crearService = crearService;
        }

        public async Task<Productos?> SubirImagen(int id, IFormFile imagen)
        {
            var producto = await _context.productos.FindAsync(id);

            if (producto == null)
                return null;

            string nombreArchivo = $"{Guid.NewGuid()}_{imagen.FileName}";
            string rutaCarpeta = Path.Combine("wwwroot", "imagenes", "productos");
            string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

            Directory.CreateDirectory(rutaCarpeta);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await imagen.CopyToAsync(stream);
            }

            producto.imagen = nombreArchivo;

            await _context.SaveChangesAsync();

            return producto;
        }
    }
}
