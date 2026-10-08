using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Cliente;
using API_SISTEMA.Models;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services
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


        public async Task<List<ClienteBuscarDto>> BuscarClientes(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return new List<ClienteBuscarDto>();

            texto = texto.Trim();

            var clientes = await _context.cliente
                .Where(c =>
                    c.nombre.Contains(texto) ||
                    c.apellido.Contains(texto) ||
                    c.nit.Contains(texto))
                .Select(c => new ClienteBuscarDto
                {
                    id_Cliente = c.id_cliente,
                    nombre = c.nombre,
                    apellido = c.apellido,
                    nit = c.nit,
                    telefono = c.telefono,
                    dpi = c.dpi,
                    correo_electronico=c.correo_electronico,
                    direccion=c.direccion,


                })
                .Take(10)
                .ToListAsync();

            return clientes;
        }

        public async Task<List<ListarClienteDto>> ListarClientes()
        {
            return await _context.cliente
                .Select(c => new ListarClienteDto
                {
                    id_Cliente = c.id_cliente,
                    nombre = c.nombre,
                    apellido = c.apellido,
                    nit = c.nit,
                    telefono = c.telefono,
                    dpi = c.dpi,
                    correo_electronico = c.correo_electronico,
                    direccion = c.direccion,
                    
                })
                .ToListAsync();
        }

    }
             
        

    
}

