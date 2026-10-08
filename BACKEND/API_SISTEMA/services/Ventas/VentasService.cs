using API_SISTEMA.data;
using API_SISTEMA.DTOs.Catalogo;
using API_SISTEMA.DTOs.Ventas;
using API_SISTEMA.models;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Contracts;
using System.Reflection.Metadata.Ecma335;
using static System.Net.Mime.MediaTypeNames;

namespace API_SISTEMA.services
{
    public class VentaService
    {
        private readonly SistemaDbContext _context;

        public VentaService(SistemaDbContext context)
        {
            _context = context;
            
        }

        public async Task<List<ListarVentasDTOs>> ListarVentas(int idUsuario)
        {
            var usuario = await _context.usuarios
                .FirstOrDefaultAsync(u => u.id_usuario == idUsuario);

            if (usuario == null)
            {
                throw new Exception("Usuario no encontrado.");
            }

            // Consulta del turno compartido; el responsable de cada operacion se conserva.
            var sesionCaja = await API_SISTEMA.services.Caja.CajaSesionActual.Consultar(_context);

            if (sesionCaja == null)
            {
                return new List<ListarVentasDTOs>();
            }

            return await _context.ventas
                .AsNoTracking()
                .Where(v =>
                    v.id_sesion_caja == sesionCaja.id_sesion_caja
                )
                .OrderByDescending(v => v.fecha_venta)
                .Select(v => new ListarVentasDTOs
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
                    ganancia_total = v.ganancia_total,
                    monto_pagado = v.monto_pagado,
                    saldo_pendiente = v.saldo_pendiente,
                    observacion = v.observacion
                })
                .ToListAsync();
        }

        //catalodo
        public async Task<List<ProductoCatalogoDTOs>> ListarCatalogo()
        {
            var productos = await _context.productos
                .AsNoTracking()
                .Select(p => new ProductoCatalogoDTOs
                {
                    id_producto = p.id_producto,
                    nombre = p.nombre,
                    imagen = p.imagen,
                    stock = p.stock??0,

                    presentaciones = p.ProductoPresentaciones
                        .Where(pp =>
                            pp.unidades_equivalentes > 0 &&
                            pp.precio > 0)
                        .OrderBy(pp => pp.unidades_equivalentes)
                        .Select(pp => new PresentacionCatalogoDTOs
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
                .OrderBy(p => p.nombre)
                .ToListAsync();

            // Validación de máximo 5 presentaciones activas
            var productoConDemasiadasPresentaciones =
                productos.FirstOrDefault(p => p.presentaciones.Count > 5);

            if (productoConDemasiadasPresentaciones != null)
            {
                throw new InvalidOperationException(
                    $"El producto '{productoConDemasiadasPresentaciones.nombre}' " +
                    "tiene más de 5 presentaciones registradas."
                );
            }

            return productos;
        }


    }
}

