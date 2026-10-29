using API_SISTEMA.Data;
using API_SISTEMA.Dtos;
using API_SISTEMA.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Dtos.Pagos;

namespace API_SISTEMA.Services
{
    public class PagoService
    {

        private readonly SistemaDbContext _context;

        public PagoService(SistemaDbContext context)
        {
            _context = context;
        }

        public async Task<List<PagoVentaHistorialDto>> ListarPorVenta(int idVenta, CancellationToken ct = default)
        {
            if (idVenta <= 0) throw new ValidationException("El identificador de la venta debe ser mayor a cero.");
            if (!await _context.ventas.AsNoTracking().AnyAsync(v => v.id_ventas == idVenta, ct))
                throw new KeyNotFoundException("La venta no existe.");

            // Incluye pago inicial y abonos; no depende de la sesión abierta ni de quién abrió la caja.
            return await _context.pagos.AsNoTracking().Where(p => p.id_venta == idVenta)
                .OrderByDescending(p => p.fecha_pago).ThenByDescending(p => p.id_pago)
                .Select(p => new PagoVentaHistorialDto
                {
                    IdPago = p.id_pago, IdVenta = p.id_venta, FechaPago = p.fecha_pago,
                    Monto = p.monto, MetodoPago = p.metodo_pago, Observacion = p.observacion,
                    IdUsuario = p.id_usuario, IdSesionCaja = p.id_sesion_caja,
                    Usuario = _context.usuarios.Where(u => u.id_usuario == p.id_usuario)
                        .Select(u => (u.nombre + " " + (u.apellido ?? "")).Trim()).FirstOrDefault() ?? "Usuario no disponible"
                }).ToListAsync(ct);
        }

       

    }
}
