using API_SISTEMA.Utilidades;
using API_SISTEMA.data;
using API_SISTEMA.DTOs.Gastos;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens.Experimental;
using API_SISTEMA.models;

namespace API_SISTEMA.services.Gastos
{
    public class CrearGastosService
    {
        private readonly SistemaDbContext _context;
        public CrearGastosService(SistemaDbContext context)
        {
            _context = context;
        }

        public async Task<int> CrearGasto(IngresarGastoDTOs gastoDto, int IdUsuario)
        {
         var sesionCaja = await _context.sesioncaja.FirstOrDefaultAsync(s =>  s.id_usuario_apertura == IdUsuario &&s.fecha_cierre == null);
            if (sesionCaja == null)
            {
                throw new Exception("No tienes una sesión de caja abierta.");
            }

            if(gastoDto.monto <= 0)
            {
                throw new Exception("El monto del gasto debe ser mayor a cero.");
            }

            if(gastoDto.descripcion == null || gastoDto.descripcion.Trim() == "")
            {
                throw new Exception("La descripción del gasto no puede estar vacía.");
            }

           
            var gasto = new models.Gastos
            {
                id_sesion_caja = sesionCaja.id_sesion_caja,
                id_usuario = IdUsuario,
                descripcion = gastoDto.descripcion,
                monto = gastoDto.monto,
                observacion = gastoDto.observacion,
                fecha = DateTime.Now
            };
            _context.gastos.Add(gasto);
            await _context.SaveChangesAsync();
            return gasto.id_gastos;
        }
    }
}
