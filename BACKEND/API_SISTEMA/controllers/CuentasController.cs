using API_SISTEMA.Services.Cuentas;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Administrador)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class CuentasController(CuentasService cuentas) : ControllerBase
{
    [HttpGet]
    public Task<CuentasPagina> Consultar([FromQuery] FiltroCuentas filtro, CancellationToken ct)
        => cuentas.Consultar(filtro, ct);
}
