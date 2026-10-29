using System.Security.Claims;
using API_SISTEMA.Dtos.Ventas;
using API_SISTEMA.Services.Ventas;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API_SISTEMA.Controllers;

[Authorize(Roles = Roles.Administrador + "," + Roles.Vendedor)]
[Route("api/[controller]")]
[ApiController]
[TypeFilter(typeof(VentaExceptionFilter))]
[EnableRateLimiting("ventas")]
[RequestSizeLimit(131072)]
public class VentaController(VentaService consulta, AbonarSaldoVentaService abonos, CrearVentaService registro,
    BuscarVentaService busqueda, ActualizarVentaService edicion) : ControllerBase
{
    private int Usuario() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0
        ? id : throw new UnauthorizedAccessException("No se pudo identificar al usuario.");

    [HttpGet("listar")]
    public async Task<IActionResult> ListarVentas([FromQuery] FiltrarVentasDto filtro, CancellationToken ct = default)
        => Ok(await consulta.ListarVentas(Usuario(), filtro, ct));

    [HttpGet("catalogo")]
    public async Task<IActionResult> ListarCatalogo(int pagina = 1, int tamanoPagina = 100, CancellationToken ct = default)
        => Ok(await consulta.ListarCatalogo(pagina, tamanoPagina, ct));

    [HttpGet("historial")]
    [Authorize(Roles = Roles.Administrador)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Historial([FromQuery] HistorialVentasFechaDto filtro,
        [FromServices] HistorialVentasFechaService historial, CancellationToken ct = default)
        => Ok(await historial.Listar(Usuario(), filtro, ct));

    [HttpGet("caja-activa/usuario/{idUsuario:int}")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> FiltrarPorUsuario(int idUsuario, [FromQuery] FiltrarVentasDto filtro,
        [FromServices] FiltrarVentasUsuarioService servicio, CancellationToken ct = default)
        => Ok(await servicio.Listar(Usuario(), idUsuario, filtro, ct));

    [HttpPost("crear")]
    public async Task<IActionResult> Crear([FromBody] CrearVentaDto dto, CancellationToken ct = default)
    {
        var venta = await registro.CrearVenta(dto, Usuario(), ct);
        return Ok(new { mensaje = "Venta registrada correctamente.", id_venta = venta.id_ventas, dto.id_operacion,
            venta.subtotal, venta.descuento, venta.total, venta.monto_pagado, venta.saldo_pendiente, venta.fecha_venta });
    }

    [HttpPost("AbonarVenta")]
    public async Task<IActionResult> AbonarVenta([FromBody] AbonarSaldoVentaDto dto, CancellationToken ct = default)
    {
        var pago = await abonos.AbonarVenta(dto, Usuario(), ct);
        return Ok(new { mensaje = "Abono registrado correctamente.", pago.id_pago, pago.id_venta, pago.monto, pago.fecha_pago, pago.id_sesion_caja });
    }

    [HttpGet("caja-activa/cliente/{idCliente:int:min(1)}")]
    public async Task<IActionResult> BuscarVentasClienteCajaActiva(int idCliente)
        => Ok(await busqueda.BuscarVentasClienteCajaActiva(Usuario(), idCliente));

    [HttpPut("modificar/{idVenta:int:min(1)}")]
    public async Task<IActionResult> ModificarVenta(int idVenta, [FromBody] ModificarVentaDto dto, CancellationToken ct = default)
    {
        await edicion.ModificarVenta(idVenta, dto, Usuario(), ct);
        return Ok(new { mensaje = "Venta modificada correctamente." });
    }
}
