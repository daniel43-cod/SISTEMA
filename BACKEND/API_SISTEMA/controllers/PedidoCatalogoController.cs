using API_SISTEMA.Dtos.PedidosCatalogo;
using API_SISTEMA.Services.PedidosCatalogo;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API_SISTEMA.Controllers;

[ApiController]
[Route("api/pedidos-catalogo")]
[AllowAnonymous]
[TypeFilter(typeof(PedidoCatalogoExceptionFilter))]
public sealed class PedidoCatalogoController(RegistrarPedidoCatalogoService service) : ControllerBase
{
    [HttpGet("zonas")]
    [EnableRateLimiting("consulta-productos")]
    public async Task<IActionResult> Zonas(CancellationToken ct) => Ok(await service.ListarZonas(ct));

    [HttpPost]
    [RequestSizeLimit(65536)]
    [EnableRateLimiting("registro-pedidos")]
    public async Task<IActionResult> Registrar(RegistrarPedidoCatalogoDto dto, CancellationToken ct)
        => Ok(await service.Registrar(dto, ct));
}
