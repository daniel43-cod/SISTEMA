using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Cliente;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services;

public class ClienteService(SistemaDbContext context)
{
    public async Task<List<ListarClienteDto>> ListarClientes(CancellationToken ct = default)
        => await context.cliente.AsNoTracking()
            .OrderBy(c => c.nombre).ThenBy(c => c.apellido).ThenBy(c => c.id_cliente)
            .Select(c => new ListarClienteDto
            {
                id_cliente = c.id_cliente,
                nombre = (c.nombre + (c.apellido == null || c.apellido == "" ? "" : " " + c.apellido)).Trim()
            }).ToListAsync(ct);

    public async Task<ClienteDto?> ObtenerCliente(int id, CancellationToken ct = default)
        => await context.cliente.AsNoTracking().Where(c => c.id_cliente == id)
            .Select(c => new ClienteDto
            {
                id_Cliente = c.id_cliente, nombre = c.nombre, apellido = c.apellido,
                nit = c.nit, dpi = c.dpi, telefono = c.telefono,
                correo_electronico = c.correo_electronico, direccion = c.direccion
            }).SingleOrDefaultAsync(ct);
}
