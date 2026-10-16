using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Gastos;
using API_SISTEMA.Services.Caja;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Gastos;

public sealed class GastoListarService(SistemaDbContext context)
{
    public async Task<GastoPaginaDto> Listar(int pagina = 1, int tamanoPagina = 25,
        CancellationToken cancellationToken = default)
    {
        if (pagina < 1 || pagina > 100000 || tamanoPagina < 1 || tamanoPagina > 100)
            throw new ValidationException("La página debe estar entre 1 y 100000 y el tamaño entre 1 y 100.");

        var sesion = await CajaSesionActual.Consultar(context, cancellationToken);
        if (sesion is null)
            return new GastoPaginaDto { Pagina = pagina, TamanoPagina = tamanoPagina };

        var consulta = context.gastos.AsNoTracking()
            .Where(g => g.id_sesion_caja == sesion.id_sesion_caja && g.sesionCaja.fecha_cierre == null);
        var total = await consulta.CountAsync(cancellationToken);
        var items = await consulta.OrderByDescending(g => g.fecha).ThenByDescending(g => g.id_gastos)
            .Skip((pagina - 1) * tamanoPagina).Take(tamanoPagina)
            .Select(g => new GastoListadoDto
            {
                IdGasto = g.id_gastos,
                Descripcion = g.descripcion,
                Monto = g.monto,
                Observacion = g.observacion,
                IdUsuario = g.id_usuario,
                Encargado = g.usuario.nombre,
                Fecha = g.fecha
            }).ToListAsync(cancellationToken);

        return new GastoPaginaDto
        {
            IdSesionCaja = sesion.id_sesion_caja,
            Pagina = pagina,
            TamanoPagina = tamanoPagina,
            Total = total,
            Items = items
        };
    }
}
