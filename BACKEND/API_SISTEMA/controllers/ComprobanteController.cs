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
        if (empresa < 1) throw new System.ComponentModel.DataAnnotations.ValidationException("Selecciona idEmpresa.");
        var negocio = await db.empresa.AsNoTracking().SingleOrDefaultAsync(e => e.id_empresa == empresa, ct)
            ?? throw new KeyNotFoundException("La empresa no existe.");
        var registro = await db.ventas.AsNoTracking().Include(v => v.cliente)
            .Include(v => v.DetalleVentas).ThenInclude(d => d.Producto)
            .Include(v => v.DetalleVentas).ThenInclude(d => d.producto_presentacion).ThenInclude(p => p.Presentacion)
            .SingleOrDefaultAsync(v => v.id_ventas == venta, ct)
            ?? throw new KeyNotFoundException("La venta no existe.");
        return (registro, negocio);
    }
}

