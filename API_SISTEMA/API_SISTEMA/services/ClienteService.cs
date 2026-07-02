using API_SISTEMA.data;
using API_SISTEMA.DTOs.Cliente;
using API_SISTEMA.models;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services
{
    public class ClienteService
    {

        private readonly SistemaDbContext _context;

        public ClienteService(SistemaDbContext context)
        {
            _context = context;
        }

        public async Task<List<Cliente>> ListarCliente()
        {
            return await _context.cliente.ToListAsync();
        }


        public async Task<List<ClienteBuscarDTOs>> BuscarClientes(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return new List<ClienteBuscarDTOs>();

            texto = texto.Trim();

            var clientes = await _context.cliente
                .Where(c =>
                    c.nombre.Contains(texto) ||
                    c.apellido.Contains(texto) ||
                    c.nit.Contains(texto))
                .Select(c => new ClienteBuscarDTOs
                {
                    id_cliente = c.id_Cliente,
                    nombre = c.nombre,
                    apellido = c.apellido,
                    nit = c.nit
                })
                .Take(10)
                .ToListAsync();

            return clientes;
        }


    }
}

