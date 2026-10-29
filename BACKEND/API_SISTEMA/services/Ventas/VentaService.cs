using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Catalogo;
using API_SISTEMA.Dtos.Ventas;
using API_SISTEMA.Models;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Contracts;
using System.Reflection.Metadata.Ecma335;
using static System.Net.Mime.MediaTypeNames;

namespace API_SISTEMA.Services.Ventas
{
    public class VentaService
    {
        private readonly SistemaDbContext _context;

        public VentaService(SistemaDbContext context)
        {
            _context = context;
            
        }

        public Task<List<ListarVentasDto>> ListarVentas(int idUsuario, FiltrarVentasDto? filtro = null, CancellationToken ct = default)
            => ConsultarVentas(idUsuario, filtro, null, ct);

        internal async Task<List<ListarVentasDto>> ConsultarVentas(int idUsuario, FiltrarVentasDto? filtro,
            int? idUsuarioFiltro, CancellationToken ct, bool soloCajaActiva = true, DateTime? hastaExclusiva = null)
            => (await ConsultarPagina(idUsuario, filtro, idUsuarioFiltro, ct, soloCajaActiva, hastaExclusiva)).Items;

        internal async Task<VentasResumenPaginaDto> ConsultarPagina(int idUsuario, FiltrarVentasDto? filtro,
            int? idUsuarioFiltro, CancellationToken ct, bool soloCajaActiva = true, DateTime? hastaExclusiva = null)
        {
            filtro ??= new FiltrarVentasDto();
            if (filtro.pagina < 1 || filtro.pagina > 100000 || filtro.tamanoPagina < 1 || filtro.tamanoPagina > 100 || filtro.desde > filtro.hasta) throw new ValidationException("Filtros de ventas inválidos.");
            var usuario = await _context.usuarios.AsNoTracking().Include(u => u.rol).FirstOrDefaultAsync(u => u.id_usuario == idUsuario, ct);

            if (usuario == null)
            {
                throw new UnauthorizedAccessException("Usuario no encontrado.");
            }

            var puedeVerGanancia = usuario.estado && usuario.rol is { estado: true, nombre: Roles.Administrador };

            // Consulta del turno compartido; el responsable de cada operacion se conserva.
            int? idSesionCaja = null;
            if (soloCajaActiva)
            {
                var sesionCaja = await API_SISTEMA.Services.Caja.CajaSesionActual.Consultar(_context, ct);
                if (sesionCaja == null) return new VentasResumenPaginaDto
                { Pagina = filtro.pagina, TamanoPagina = filtro.tamanoPagina, GananciaTotal = puedeVerGanancia ? 0m : null };
                idSesionCaja = sesionCaja.id_sesion_caja;
            }

            var consulta = _context.ventas
                .AsNoTracking()
                .Where(v =>
                    !soloCajaActiva || v.id_sesion_caja == idSesionCaja
                )
                .Where(v => !idUsuarioFiltro.HasValue || v.id_usuario == idUsuarioFiltro.Value)
                .Where(v => !hastaExclusiva.HasValue || v.fecha_venta < hastaExclusiva.Value)
                .Where(v => (!filtro.desde.HasValue || v.fecha_venta >= filtro.desde.Value) && (!filtro.hasta.HasValue || v.fecha_venta <= filtro.hasta.Value));
            var total = await consulta.CountAsync(ct);
            decimal? ganancia = puedeVerGanancia
                ? await consulta.SumAsync(v => (decimal?)v.ganancia_total, ct) ?? 0m : null;
            var items = await consulta
                .OrderByDescending(v => v.fecha_venta).ThenByDescending(v => v.id_ventas)
                .Skip((filtro.pagina - 1) * filtro.tamanoPagina).Take(filtro.tamanoPagina)
                .Select(v => new ListarVentasDto
                {
                    id_ventas = v.id_ventas,
                    subtotal = v.subtotal,
                    descuento = v.descuento,
                    impuesto = v.impuesto,
                    total = v.total,
                    fecha_venta = v.fecha_venta,
                    id_cliente = v.id_cliente,
                    cliente = v.cliente.nombre,
                    id_usuario = v.id_usuario,
                    usuario = v.usuario.nombre,
                    estado = v.EstadoVenta.descripcion,
                    origen = v.origen,
                    ganancia_total = puedeVerGanancia ? v.ganancia_total : (decimal?)null,
                    monto_pagado = v.monto_pagado,
                    saldo_pendiente = v.saldo_pendiente,
                    observacion = v.observacion
                })
                .ToListAsync(ct);
            return new VentasResumenPaginaDto
            {
                Pagina = filtro.pagina, TamanoPagina = filtro.tamanoPagina, TotalRegistros = total,
                HayMas = (long)filtro.pagina * filtro.tamanoPagina < total, GananciaTotal = ganancia, Items = items
            };
        }

        //catalodo
        public async Task<List<ProductoCatalogoDto>> ListarCatalogo(int pagina = 1, int tamanoPagina = 100, CancellationToken ct = default)
        {
            if (pagina < 1 || pagina > 100000 || tamanoPagina < 1 || tamanoPagina > 100) throw new ValidationException("Paginación inválida.");
            var productos = await _context.productos
                .AsNoTracking()
                .Where(p => p.Marca.Estado && p.Marca.Categoria.Estado && p.PrecioPromedioCompra > 0)
                .OrderBy(p => p.Nombre).ThenBy(p => p.IdProducto).Skip((pagina - 1) * tamanoPagina).Take(tamanoPagina)
                .Select(p => new ProductoCatalogoDto
                {
                    id_producto = p.IdProducto,
                    nombre = p.Nombre,
                    imagen = p.Imagen,
                    stock = p.Stock??0,

                    presentaciones = p.ProductoPresentaciones
                        .Where(pp =>
                            pp.estado && pp.Presentacion.Estado == true && pp.unidades_equivalentes > 0 &&
                            pp.precio > p.PrecioPromedioCompra * pp.unidades_equivalentes)
                        .OrderBy(pp => pp.unidades_equivalentes)
                        .Select(pp => new PresentacionCatalogoDto
                        {
                            id_producto_presentacion =
                                pp.id_producto_presentacion,

                            presentacion = pp.Presentacion.Descripcion,

                            unidades_equivalentes =
                                pp.unidades_equivalentes,

                            precio = pp.precio
                        })
                        .ToList()
                })

                .ToListAsync(ct);

            return productos;
        }


    }
}

