using API_SISTEMA.Dtos.PedidosCatalogo;
using API_SISTEMA.Services.PedidosCatalogo;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API_SISTEMA.Controllers;

[ApiController]
[Route("api/pedidos-catalogo")]
[TypeFilter(typeof(PedidoCatalogoExceptionFilter))]
public sealed class PedidoCatalogoController(RegistrarPedidoCatalogoService service) : ControllerBase
{
    [HttpGet("zonas")]
    [AllowAnonymous]
    [EnableRateLimiting("consulta-productos")]
    public async Task<IActionResult> Zonas(CancellationToken ct) => Ok(await service.ListarZonas(ct));

    [HttpPost]
    [AllowAnonymous]
    [RequestSizeLimit(65536)]
    [EnableRateLimiting("registro-pedidos")]
    public async Task<IActionResult> Registrar(RegistrarPedidoCatalogoDto dto, CancellationToken ct)
        => Ok(await service.Registrar(dto, ct));

    [HttpGet("pendientes")]
    [Authorize(Roles = Roles.Administrador)]
    [EnableRateLimiting("consulta-productos")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Pendientes([FromServices] ListarPedidosCatalogoService listado,
        [FromQuery] int pagina = 1, CancellationToken ct = default)
        => Ok(await listado.ListarPendientes(pagina, ct));

    [HttpGet("{idPedido:int}/detalle")]
    [Authorize(Roles = Roles.Administrador)]
    [EnableRateLimiting("consulta-productos")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Detalle(int idPedido, [FromServices] ListarPedidosCatalogoService listado,
        CancellationToken ct = default)
    {
        var pedido = await listado.ObtenerDetalle(idPedido, ct);
        return pedido is null ? NotFound(new { mensaje = "El pedido no existe." }) : Ok(pedido);
    }

    [HttpPut("{idPedido:int}/detalles")]
    [Authorize(Roles = Roles.Administrador)]
    [EnableRateLimiting("registro-pedidos")]
    [RequestSizeLimit(65536)]
    public async Task<IActionResult> EditarDetalles(int idPedido, EditarPedidoCatalogoDto dto,
        [FromServices] EditarPedidoCatalogoService edicion, CancellationToken ct = default)
    {
        await edicion.Editar(idPedido, dto, ct);
        return Ok(new { mensaje = "Detalles del pedido actualizados correctamente." });
    }

    [HttpPost("{idPedido:int}/confirmar-venta")]
    [Authorize(Roles = Roles.Administrador)]
    [EnableRateLimiting("ventas")]
    [RequestSizeLimit(65536)]
    public async Task<IActionResult> ConfirmarVenta(int idPedido, ConfirmarVentaPedidoDto dto,
        [FromServices] ConfirmarVentaPedidoService confirmacion, CancellationToken ct = default)
        => Ok(await confirmacion.Confirmar(idPedido, dto, ct));
}
