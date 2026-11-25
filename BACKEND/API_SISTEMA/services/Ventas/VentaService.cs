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
            if (filtro.idCliente is <= 0 || (filtro.idCliente.HasValue && !soloCajaActiva) ||
                (filtro.buscar?.Length ?? 0) > 100 ||
                (filtro.estadoPago is not null && filtro.estadoPago != "pendientes" && filtro.estadoPago != "pagadas"))
                throw new ValidationException("Filtros de ventas inválidos.");
            var usuario = await _context.usuarios.AsNoTracking().Include(u => u.rol).FirstOrDefaultAsync(u => u.id_usuario == idUsuario, ct);

            if (usuario is null || !usuario.estado || usuario.rol is null || !usuario.rol.estado ||
                (usuario.rol.nombre != Roles.Administrador && usuario.rol.nombre != Roles.Vendedor))
            {
                throw new UnauthorizedAccessException("No tienes permiso para consultar estas ventas.");
            }

            var puedeVerGanancia = usuario.estado && usuario.rol is { estado: true, nombre: Roles.Administrador };
            // El turno es compartido; el vendedor solo puede consultar sus propias ventas.
            if (usuario.rol.nombre == Roles.Vendedor)
            {
                if (idUsuarioFiltro.HasValue && idUsuarioFiltro.Value != idUsuario)
                    throw new UnauthorizedAccessException("Solo un administrador puede consultar ventas de otro usuario.");
                idUsuarioFiltro = idUsuario;
            }

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
            // Las opciones de clientes pertenecen al turno activo y respetan el alcance del usuario.
            var clientes = soloCajaActiva
                ? await _context.ventas.AsNoTracking()
                    .Where(v => v.id_sesion_caja == idSesionCaja && (puedeVerGanancia || v.id_usuario == idUsuario))
                    .Select(v => new { v.id_cliente, v.cliente.nombre }).Distinct()
                    .OrderBy(v => v.nombre)
                    .Select(v => new ClienteVentasCajaDto { IdCliente = v.id_cliente, Nombre = v.nombre })
                    .ToListAsync(ct)
                : new List<ClienteVentasCajaDto>();
            if (filtro.idCliente.HasValue)
                consulta = consulta.Where(v => v.id_cliente == filtro.idCliente.Value);
            if (!string.IsNullOrWhiteSpace(filtro.buscar))
            {
                var texto = filtro.buscar.Trim();
                var esId = int.TryParse(texto.TrimStart('#'), out var numeroVenta);
                consulta = consulta.Where(v => v.cliente.nombre.Contains(texto) || (esId && v.id_ventas == numeroVenta));
            }
            var pendientes = await consulta.CountAsync(v => v.saldo_pendiente > 0, ct);
            var pagadas = await consulta.CountAsync(v => v.saldo_pendiente == 0, ct);
            if (filtro.estadoPago == "pendientes") consulta = consulta.Where(v => v.saldo_pendiente > 0);
            if (filtro.estadoPago == "pagadas") consulta = consulta.Where(v => v.saldo_pendiente == 0);
            var porCobrar = await consulta.SumAsync(v => (decimal?)v.saldo_pendiente, ct) ?? 0m;
            var total = await consulta.CountAsync(ct);
            var totalVentas = await consulta.SumAsync(v => (decimal?)v.total, ct) ?? 0m;
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
                    tipo_identificacion_receptor = v.tipo_identificacion_receptor,
                    identificacion_receptor = v.identificacion_receptor, nombre_receptor = v.nombre_receptor,
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
                HayMas = (long)filtro.pagina * filtro.tamanoPagina < total, GananciaTotal = ganancia,
                TotalVentas = totalVentas, TotalPorCobrar = porCobrar, CantidadPendientes = pendientes, CantidadPagadas = pagadas, ClientesCajaActiva = clientes, Items = items
            };
        }

        public async Task<List<SurtidoVentaCatalogoDto>> ListarCatalogoSurtidos(int pagina = 1, int tamanoPagina = 100, CancellationToken ct = default, int? idMarca = null)
        {
            if (pagina < 1 || pagina > 100000 || tamanoPagina < 1 || tamanoPagina > 100) throw new ValidationException("Paginación inválida.");
            var grupos = await _context.Surtidos.AsNoTracking().Include(s => s.Productos).ThenInclude(p => p.Producto)
                .ThenInclude(p => p.Marca).ThenInclude(m => m.Categoria)
                .Where(s => s.Estado && s.Marca.Estado && s.Marca.Categoria.Estado)
                .Where(s => !idMarca.HasValue || s.IdMarca == idMarca.Value)
                .OrderBy(s => s.Nombre).ThenBy(s => s.IdSurtido).Skip((pagina - 1) * tamanoPagina).Take(tamanoPagina).ToListAsync(ct);
            var ids = grupos.Select(s => s.IdSurtido).ToArray();
            var presentaciones = await _context.producto_presentaciones.AsNoTracking().Include(p => p.Presentacion)
                .Where(p => p.id_surtido.HasValue && ids.Contains(p.id_surtido.Value) && p.estado && p.Presentacion.Estado == true).ToListAsync(ct);
            return grupos.Select(s =>
            {
                var productos = s.Productos.Select(p => p.Producto).ToArray();
                string? bloqueo = null;
                try
                {
                    API_SISTEMA.Services.Surtidos.SurtidoReglas.ValidarCostosPromedio(productos);
                    if (productos.Any(p => !p.Marca.Estado || !p.Marca.Categoria.Estado)) bloqueo = "Hay componentes con marca o categoría inactiva.";
                }
                catch (ValidationException error) { bloqueo = error.Message; }
                return new SurtidoVentaCatalogoDto
                {
                    id_surtido = s.IdSurtido, nombre = s.Nombre, imagen = s.Imagen, bloqueo = bloqueo,
                    stock = (int)Math.Min(int.MaxValue, productos.Sum(p => (long)Math.Max(0, p.Stock ?? 0))),
                    componentes = productos.Select(p => new ComponenteSurtidoCatalogoDto(p.IdProducto, p.Nombre, Math.Max(0, p.Stock ?? 0))).ToList(),
                    presentaciones = presentaciones.Where(p => p.id_surtido == s.IdSurtido).Select(p => new PresentacionCatalogoDto
                    {
                        id_producto_presentacion = p.id_producto_presentacion, presentacion = p.Presentacion.Descripcion,
                        unidades_equivalentes = p.unidades_equivalentes, precio = p.precio
                    }).ToList()
                };
            }).ToList();
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

