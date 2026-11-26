using API_SISTEMA.Data;

using API_SISTEMA.Services.Comprobantes;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Controllers;

[ApiController, Route("api/comprobantes")]
[Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
[EnableRateLimiting("ventas")]
[TypeFilter(typeof(VentaExceptionFilter))]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class ComprobanteController(SistemaDbContext db) : ControllerBase
{
    [HttpGet("ventas/{idVenta:int:min(1)}/opciones-whatsapp")]
    public async Task<IActionResult> OpcionesWhatsApp(int idVenta, CancellationToken ct)
    {
        await API_SISTEMA.Services.Ventas.AccesoVenta.Validar(db, idVenta, Usuario(), ct);
        var venta = await db.ventas.AsNoTracking().Include(v => v.cliente).SingleOrDefaultAsync(v => v.id_ventas == idVenta, ct)
            ?? throw new KeyNotFoundException("La venta no existe.");
        var empresas = await db.empresa.AsNoTracking().OrderBy(e => e.id_empresa)
            .Select(e => new { id = e.id_empresa, nombre = e.nombre_empresa }).ToListAsync(ct);
        return Ok(new { telefono = API_SISTEMA.Services.WhatsApp.WhatsAppHistorialService.TelefonoRegistrado(venta), empresas });
    }

    [HttpPost("ventas/{idVenta:int:min(1)}/whatsapp/registro")]
    public async Task<IActionResult> EnviarRegistro(int idVenta, [FromQuery] int idEmpresa,
        [FromBody] API_SISTEMA.Dtos.Ventas.EnviarComprobanteTelefonoDto dto,
        [FromServices] API_SISTEMA.Services.WhatsApp.WhatsAppHistorialService servicio, CancellationToken ct)
    {
        var datos = await Consultar(idVenta, idEmpresa, ct);
        await servicio.GuardarTelefonoRegistro(datos.Venta, dto.Telefono, ct);
        return Ok(await servicio.Iniciar(datos.Venta, datos.Empresa, Usuario(), ct));
    }

    [HttpPost("ventas/{idVenta:int:min(1)}/whatsapp/historial")]
    public async Task<IActionResult> EnviarHistorial(int idVenta, [FromQuery] int idEmpresa,
        [FromBody] API_SISTEMA.Dtos.Ventas.EnviarComprobanteTelefonoDto dto,
        [FromServices] API_SISTEMA.Services.WhatsApp.WhatsAppHistorialService servicio, CancellationToken ct)
    {
        var datos = await Consultar(idVenta, idEmpresa, ct);
        return Ok(await servicio.Iniciar(datos.Venta, datos.Empresa, Usuario(), ct, dto.Telefono));
    }

    [HttpPost("ventas/{idVenta:int:min(1)}/whatsapp")]
    public async Task<IActionResult> EnviarWhatsApp(int idVenta, [FromQuery] int idEmpresa,
        [FromServices] API_SISTEMA.Services.WhatsApp.WhatsAppHistorialService servicio, CancellationToken ct)
    {
        var datos = await Consultar(idVenta, idEmpresa, ct);
        var resultado = await servicio.Iniciar(datos.Venta, datos.Empresa, Usuario(), ct);
        return Ok(resultado);
    }

    [HttpPost("ventas/{idVenta:int:min(1)}/pagos/whatsapp")]
    public async Task<IActionResult> EnviarPagos(int idVenta, [FromQuery] int idEmpresa,
        [FromServices] API_SISTEMA.Services.WhatsApp.WhatsAppHistorialService servicio, CancellationToken ct,
        [FromBody] API_SISTEMA.Dtos.Ventas.EnviarComprobanteTelefonoDto? dto = null)
    {
        var datos = await Consultar(idVenta, idEmpresa, ct);
        return Ok(await servicio.IniciarPagos(datos.Venta, datos.Empresa, Usuario(), ct, dto?.Telefono));
    }

    [HttpGet("ventas/{idVenta:int:min(1)}/pagos/imagen")]
    public async Task<IActionResult> ImagenPagos(int idVenta, [FromQuery] int idEmpresa,
        [FromQuery] int pagina = 1, CancellationToken ct = default)
    {
        var datos = await Consultar(idVenta, idEmpresa, ct);
        var pagos = await new API_SISTEMA.Services.PagoService(db).ListarPorVenta(idVenta, Usuario(), ct);
        var paginas = ComprobanteImagen.PlanificarPagos(datos.Venta, datos.Empresa, pagos);
        if (pagina < 1 || pagina > paginas.Count) return NotFound(new { mensaje = "La página no existe." });
        return File(ComprobanteImagen.Generar(datos.Venta, datos.Empresa, paginas, pagina, true), "image/png");
    }

    private int Usuario() => int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) && id > 0
        ? id : throw new UnauthorizedAccessException("No se pudo identificar al usuario.");

    [HttpGet("whatsapp/{idEnvio:guid}")]
    public async Task<IActionResult> EstadoWhatsApp(Guid idEnvio,
        [FromServices] API_SISTEMA.Services.WhatsApp.WhatsAppHistorialService servicio, CancellationToken ct)
    {
        var venta = await db.WhatsAppEnvios.AsNoTracking().Where(e => e.id_envio == idEnvio).Select(e => (int?)e.id_venta).SingleOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException("El envío no existe.");
        await API_SISTEMA.Services.Ventas.AccesoVenta.Validar(db, venta, Usuario(), ct);
        return Ok(await servicio.Consultar(idEnvio, ct));
    }

    [HttpGet("ventas/{idVenta:int:min(1)}/whatsapp/historial")]
    public async Task<IActionResult> Historial(int idVenta, [FromQuery] int idEmpresa,
        [FromServices] API_SISTEMA.Services.WhatsApp.WhatsAppHistorialService servicio, CancellationToken ct,
        [FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 20)
    {
        await API_SISTEMA.Services.Ventas.AccesoVenta.Validar(db, idVenta, Usuario(), ct);
        return Ok(await servicio.Historial(idVenta, idEmpresa, ct, pagina, tamanoPagina));
    }

    [HttpGet("ventas/{idVenta:int:min(1)}/paginas")]
    public async Task<IActionResult> Paginas(int idVenta, [FromQuery] int idEmpresa, CancellationToken ct)
    {
        var datos = await Consultar(idVenta, idEmpresa, ct);
        return Ok(new { paginas = ComprobanteImagen.Planificar(datos.Venta, datos.Empresa).Count,
            ancho = ComprobanteImagen.Ancho, formato = "image/png" });
    }

    [HttpGet("ventas/{idVenta:int:min(1)}/imagen")]
    public async Task<IActionResult> Imagen(int idVenta, [FromQuery] int idEmpresa,
        [FromQuery] int pagina = 1, CancellationToken ct = default)
    {
        var datos = await Consultar(idVenta, idEmpresa, ct);
        var paginas = ComprobanteImagen.Planificar(datos.Venta, datos.Empresa);
        if (pagina < 1 || pagina > paginas.Count) return NotFound(new { mensaje = "La página no existe." });
        var png = ComprobanteImagen.Generar(datos.Venta, datos.Empresa, paginas, pagina);
        return File(png, "image/png");
    }

    private async Task<(Models.Ventas Venta, Models.Empresa Empresa)> Consultar(int venta, int empresa, CancellationToken ct)
    {
        await API_SISTEMA.Services.Ventas.AccesoVenta.Validar(db, venta, Usuario(), ct);
        if (empresa < 1) throw new System.ComponentModel.DataAnnotations.ValidationException("Selecciona idEmpresa.");
        var negocio = await db.empresa.AsNoTracking().SingleOrDefaultAsync(e => e.id_empresa == empresa, ct)
            ?? throw new KeyNotFoundException("La empresa no existe.");
        var registro = await db.ventas.AsNoTracking().Include(v => v.cliente)
            .Include(v => v.DetalleVentas).ThenInclude(d => d.Producto)
            .Include(v => v.DetalleVentas).ThenInclude(d => d.Surtido)
            .Include(v => v.DetalleVentas).ThenInclude(d => d.producto_presentacion).ThenInclude(p => p.Presentacion)
            .SingleOrDefaultAsync(v => v.id_ventas == venta, ct)
            ?? throw new KeyNotFoundException("La venta no existe.");
        return (registro, negocio);
    }
}

