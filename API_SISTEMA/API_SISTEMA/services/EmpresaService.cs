using API_SISTEMA.data;
using API_SISTEMA.DTOs.Empresa;
using API_SISTEMA.DTOs.EmpresaDTOs;
using API_SISTEMA.models;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services
{
    public class EmpresaService
    {
        private readonly SistemaDbContext _context;
        public EmpresaService(SistemaDbContext context)
        {
            _context = context;
        }





        public async Task<Empresa> CrearEmpresa(EmpresaDTOs empresaDto)
        {
            if (empresaDto == null)
                throw new Exception("Los datos de la empresa son obligatorios.");

            if (string.IsNullOrWhiteSpace(empresaDto.nombre_empresa))
                throw new Exception("El nombre de la empresa es obligatorio.");

            if (string.IsNullOrWhiteSpace(empresaDto.nit))
                throw new Exception("El NIT de la empresa es obligatorio.");

            bool nitExiste = await _context.empresa.AnyAsync(e => e.nit == empresaDto.nit);

            if (nitExiste)
                throw new Exception("Ya existe una empresa registrada con ese NIT.");

            var empresa = new Empresa
            {
                nombre_empresa = empresaDto.nombre_empresa.Trim(),
                nit = empresaDto.nit.Trim()
            };

            _context.empresa.Add(empresa);
            await _context.SaveChangesAsync();

            return empresa;
        }
    }
}

