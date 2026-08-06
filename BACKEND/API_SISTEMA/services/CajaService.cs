using API_SISTEMA.data;
using API_SISTEMA.DTOs.Caja;
using API_SISTEMA.models;
using API_SISTEMA.data;
using Microsoft.EntityFrameworkCore;
namespace API_SISTEMA.services
{
    public class CajaService
    {
        private readonly SistemaDbContext _context;

        public CajaService(SistemaDbContext context)
        {
            _context = context;
        }

        public async Task<SesionCaja> AbrirCaja( AperturaCajaDTOs caja,int idUsuario)
        {
            var usuarioExiste = await _context.usuarios.AnyAsync(u => u.id_usuario == idUsuario);

            if (!usuarioExiste)
                throw new Exception("El usuario autenticado no existe.");

            var cajaExiste = await _context.caja
                .AnyAsync(c => c.id_caja == caja.id_caja);

            if (!cajaExiste)
                throw new Exception("La caja seleccionada no existe.");

            var cajaYaAbierta = await _context.sesioncaja
                .AnyAsync(s =>
                    s.id_caja == caja.id_caja &&
                    s.fecha_cierre == null);

            if (cajaYaAbierta)
                throw new Exception("Esta caja ya tiene una sesión abierta.");

            var sesionUsuarioAbierta = await _context.sesioncaja
                .AnyAsync(s =>
                    s.id_usuario_apertura == idUsuario &&
                    s.fecha_cierre == null);

            if (sesionUsuarioAbierta)
                throw new Exception("El usuario ya tiene una sesión de caja abierta.");

            if (caja.monto_inicial < 0)
                throw new Exception("El monto inicial no puede ser negativo.");

            var cajaApertura = new SesionCaja
            {
                id_caja = caja.id_caja,
                id_usuario_apertura = idUsuario,
                fecha_apertura = DateTime.Now,
                monto_inicial = caja.monto_inicial,
                monto_esperado = caja.monto_inicial,
                observacion_apertura = caja.observacion
            };

            _context.sesioncaja.Add(cajaApertura);
            await _context.SaveChangesAsync();

            return cajaApertura;
        }


        public async Task<SesionCaja> CerrarCaja(CierreCajaDTOs dto,int idUsuario)
        {
            var sesionCaja = await _context.sesioncaja
                .Where(s =>
                    s.id_usuario_apertura == idUsuario &&
                    s.fecha_cierre == null)
                .OrderByDescending(s => s.fecha_apertura).FirstOrDefaultAsync();

            if (sesionCaja == null)
            {
                throw new Exception("El usuario no tiene una sesión de caja abierta.");
            }

            if (dto.monto_contado < 0)
            {
                throw new Exception("El monto contado no puede ser negativo.");
            }

            var totalVentas = await _context.ventas
                .Where(v =>
                    v.id_sesion_caja == sesionCaja.id_sesion_caja)
                .SumAsync(v => (decimal?)v.total) ?? 0;

            var totalPagadoVentas = await _context.ventas
                .Where(v =>
                    v.id_sesion_caja == sesionCaja.id_sesion_caja)
                .SumAsync(v => (decimal?)v.monto_pagado) ?? 0;

            sesionCaja.id_usuario_cierre = idUsuario;
            sesionCaja.fecha_cierre = DateTime.Now;

            // Para la caja importa el dinero recibido, no todo lo facturado.
            sesionCaja.monto_esperado =
                sesionCaja.monto_inicial + totalPagadoVentas;

            sesionCaja.monto_contado = dto.monto_contado;

            sesionCaja.diferencia =
                sesionCaja.monto_contado -
                sesionCaja.monto_esperado;

            sesionCaja.observacion_cierre =
                dto.observacion_cierre;

            await _context.SaveChangesAsync();

            return sesionCaja;
        }

        //Listar solo una caja
            public async  Task<List<ListarSesionesDTOs>> ListarSesionesCaja(int idCaja)
            {
                var sesiones = await _context.sesioncaja
                    .Where(s => s.id_caja == idCaja)
                    .Select(s => new ListarSesionesDTOs
                    {
                        id_sesion_caja = s.id_sesion_caja,  
                        id_caja = s.id_caja,
                        id_usuario_apertura = s.id_usuario_apertura,
                        id_usuario_cierre = s.id_usuario_cierre,
                        fecha_apertura = s.fecha_apertura,
                        fecha_cierre = s.fecha_cierre,
                        monto_inicial = s.monto_inicial,
                        monto_contado = s.monto_contado,
                        diferencia = s.diferencia,
                        observacion_apertura = s.observacion_apertura,
                        observacion_cierre = s.observacion_cierre
                    })
                    .ToListAsync();
                return sesiones;
            }

        //listar todas las sesiones de la caja

        public async Task<List<ListarSesionesDTOs>> ListarSesionesCaja()
        {
            var sesiones = await _context.sesioncaja
                .Select(s => new ListarSesionesDTOs
                {
                    id_sesion_caja = s.id_sesion_caja,
                    id_caja = s.id_caja,
                    id_usuario_apertura = s.id_usuario_apertura,
                    usuario_apertura = s.usuarioapertura.nombre,
                    id_usuario_cierre = s.id_usuario_cierre,
                    usuario_cierre = s.usuariocierre.nombre,
                    fecha_apertura = s.fecha_apertura,
                    fecha_cierre = s.fecha_cierre,
                    monto_inicial = s.monto_inicial,
                    monto_contado = s.monto_contado,
                    diferencia = s.diferencia,
                    observacion_apertura = s.observacion_apertura,
                    observacion_cierre = s.observacion_cierre
                })
                .ToListAsync();

            return sesiones;
        }

    }
}
