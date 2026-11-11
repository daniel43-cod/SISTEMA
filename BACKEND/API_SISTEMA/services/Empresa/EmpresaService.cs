using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Empresa;
using API_SISTEMA.Models;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;
namespace API_SISTEMA.Services;
public sealed class EmpresaService(SistemaDbContext context)
{
    public async Task<Empresa> EditarEmpresa(EditarEmpresaDto dto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        Validator.ValidateObject(dto, new ValidationContext(dto), true);
        var empresas = await context.empresa.OrderBy(e => e.id_empresa).Take(2).ToListAsync(ct);
        if (empresas.Count == 0) throw new KeyNotFoundException("No existe una empresa configurada.");
        if (empresas.Count > 1) throw new VentaConflictoException("Hay varias empresas registradas. Corrige la configuración antes de editar.");
        static string? Texto(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        var empresa = empresas[0];
        empresa.nombre_empresa = dto.nombre_empresa.Trim();
        empresa.nit = dto.nit.Trim();
        empresa.ubicacion = Texto(dto.ubicacion);
        empresa.direccion = Texto(dto.direccion);
        empresa.telefonos = Texto(dto.telefonos);
        empresa.razon_social = Texto(dto.razon_social);
        empresa.nombre_comercial = Texto(dto.nombre_comercial);
        empresa.afiliacion_iva = Texto(dto.afiliacion_iva)?.ToUpperInvariant();
        empresa.codigo_establecimiento = dto.codigo_establecimiento;
        empresa.correo_emisor = Texto(dto.correo_emisor);
        empresa.codigo_postal = Texto(dto.codigo_postal);
        empresa.municipio = Texto(dto.municipio);
        empresa.departamento = Texto(dto.departamento);
        empresa.pais = Texto(dto.pais)?.ToUpperInvariant();
        await context.SaveChangesAsync(ct);
        return empresa;
    }
}
