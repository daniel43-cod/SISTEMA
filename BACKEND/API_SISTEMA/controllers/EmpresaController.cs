using API_SISTEMA.Dtos.Empresa;
using API_SISTEMA.Services;
using API_SISTEMA.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace API_SISTEMA.Controllers;
[Authorize(Roles = Roles.Administrador)]
[Route("api/[controller]")]
[ApiController]
[TypeFilter(typeof(VentaExceptionFilter))]
public sealed class EmpresaController(EmpresaService servicio) : ControllerBase
{
    [HttpPut("editar")]
    public async Task<IActionResult> Editar([FromBody] EditarEmpresaDto dto, CancellationToken ct)
    {
        var empresa = await servicio.EditarEmpresa(dto, ct);
        return Ok(new { mensaje = "Empresa actualizada correctamente.", empresa });
    }
}
