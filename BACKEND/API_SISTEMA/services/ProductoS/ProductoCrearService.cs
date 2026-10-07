using API_SISTEMA.services.Auditoria;
﻿using System.ComponentModel.DataAnnotations;
using System.Data;
using API_SISTEMA.data;
using API_SISTEMA.DTOs.Productos;
using API_SISTEMA.models;
using API_SISTEMA.services.ProductoS;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.services;

public class ProductoCrearService
{
    private readonly ProductoAuditoriaService _auditoria;
    private readonly SistemaDbContext _context;
    private readonly ProductoImagenService _imagenes;
    public ProductoCrearService(SistemaDbContext context, ProductoImagenService imagenes, ProductoAuditoriaService auditoria)
    {
        _auditoria = auditoria;
        _context = context;
        _imagenes = imagenes;
    }

    // Validación independiente del controlador para proteger también llamadas internas.
    public static void Validar(productocrear dto)
    {
        if (dto is null) throw new ValidationException("La información del producto es obligatoria.");
        Validator.ValidateObject(dto, new ValidationContext(dto), true);
        if (dto.codigo_barra.Any(char.IsControl) || dto.codigo_barra.Trim().Any(char.IsWhiteSpace))
            throw new ValidationException("El código de barras no admite espacios internos ni caracteres de control.");
        if (dto.nombre.Any(char.IsControl))
            throw new ValidationException("El nombre no admite caracteres de control.");
        foreach (var item in dto.presentaciones)
        {
            if (item is null) throw new ValidationException("Hay una presentación vacía.");
            Validator.ValidateObject(item, new ValidationContext(item), true);
            if (decimal.Round(item.precio, 2) != item.precio)
                throw new ValidationException("El precio de venta admite hasta dos decimales.");
        }
        if (dto.presentaciones.Select(p => p.id_presentacion).Distinct().Count() != dto.presentaciones.Count)
            throw new ValidationException("No puedes repetir una presentación en el mismo producto.");
    }

    public async Task<Productos> CrearProducto(productocrear dto,
        CancellationToken cancellationToken = default, IFormFile? imagen = null)
    {
        Validar(dto);
        var url = _imagenes.ValidarUrl(dto.UrlImagen);
        if (url != null && imagen != null)
            throw new ValidationException("Elige un enlace o un archivo, no ambos.");
        string? archivo = null;
        bool commitIniciado = false;
        try
        {
            if (imagen != null) url = archivo = await _imagenes.GuardarAsync(imagen, cancellationToken);
            // Serializa la verificación del código y el alta. Todo el producto se guarda junto.
            await using var transaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, cancellationToken);
            // La categoría se valida mediante la marca, sin almacenarla en Productos.
            var marcaValida = await _context.Marcas.AnyAsync(
                m => m.IdMarca == dto.IdMarca && m.Estado && m.Categoria.Estado, cancellationToken);
            if (!marcaValida) throw new ValidationException("La marca y su categoría deben existir y estar activas.");
            var codigo = dto.codigo_barra.Trim();
            var normalizado = codigo.ToUpperInvariant();
            if (await _context.productos.AnyAsync(p => p.codigo_barra != null &&
                p.codigo_barra.Trim().ToUpper() == normalizado, cancellationToken))
                throw new ProductoDuplicadoException();
            var nombreNormalizado = dto.nombre.Trim().ToUpperInvariant();
            if (await _context.productos.AnyAsync(p => p.nombre.Trim().ToUpper() == nombreNormalizado,
                cancellationToken))
                throw new ProductoDuplicadoException("Ya existe un producto con ese nombre.");
            var ids = dto.presentaciones.Select(p => p.id_presentacion).ToList();
            if (await _context.presentaciones.CountAsync(p => ids.Contains(p.IdPresentacion) &&
                p.Estado == true, cancellationToken) != ids.Count)
                throw new ValidationException("Todas las presentaciones deben existir y estar activas.");

            var producto = new Productos
            {
                codigo_barra = codigo, nombre = dto.nombre.Trim(), IdMarca = dto.IdMarca,
                stock = 0, stock_minimo = dto.stock_minimo, imagen = url,
                costo_unitario = null, fecha_creacion = DateTime.Now,
                ProductoPresentaciones = dto.presentaciones.Select(p => new Producto_Presentacion
                {
                    IdPresentacion = p.id_presentacion, unidades_equivalentes = p.unidades_equivalentes,
                    precio = p.precio, estado = true
                }).ToList()
            };
            _context.productos.Add(producto);
            await _context.SaveChangesAsync(cancellationToken);
            await _auditoria.Registrar(producto, null, cancellationToken);
            foreach (var presentacion in producto.ProductoPresentaciones)
                await _auditoria.RegistrarPresentacion(presentacion, null, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            commitIniciado = true;
            await transaction.CommitAsync(cancellationToken);
            return producto;
        }
        catch
        {
            // No borrar si el resultado del commit es incierto.
            if (archivo != null && !commitIniciado) _imagenes.Eliminar(archivo);
            throw;
        }
    }
}
