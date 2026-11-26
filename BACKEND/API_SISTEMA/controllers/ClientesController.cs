using System.Security.Claims;
using API_SISTEMA.Dtos.Cliente;
using API_SISTEMA.Services.Clientes;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.Controllers;

[ApiController]
[Route("api/clientes")]
[Authorize(Roles = Roles.Administrador)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[TypeFilter(typeof(ClienteExceptionFilter))]
public sealed class ClientesController(HistorialClienteService service) : ControllerBase
{
    private int Usuario() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0
        ? id : throw new UnauthorizedAccessException("No se pudo identificar al usuario.");

    [HttpGet]
    public Task<ClientesPagina> Listar([FromQuery] ListadoClientesFiltro filtro, CancellationToken ct)
        => service.Listar(Usuario(), filtro, ct);

    [HttpGet("{idCliente}/resumen")]
    public Task<ClienteResumen> Resumen(int idCliente, CancellationToken ct)
        => service.Resumen(Usuario(), idCliente, ct);

    [HttpGet("{idCliente}/ventas")]
    public Task<ClienteVentasPagina> Historial(int idCliente, [FromQuery] PaginaClientesFiltro filtro, CancellationToken ct)
        => service.Historial(Usuario(), idCliente, filtro, ct);
}
