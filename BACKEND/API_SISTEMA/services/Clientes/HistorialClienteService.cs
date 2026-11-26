using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Cliente;
using API_SISTEMA.Services.Ventas;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Services.Clientes;

public sealed class HistorialClienteService(SistemaDbContext db, TimeProvider? reloj = null)
{
    private async Task Administrador(int usuario, CancellationToken ct)
    {
        if (!await AccesoVenta.EsAdministrador(db, usuario, ct))
            throw new UnauthorizedAccessException("Solo un administrador puede consultar el historial de clientes.");
    }

    public async Task<ClientesPagina> Listar(int usuario, ListadoClientesFiltro filtro, CancellationToken ct = default)
    {
        await Administrador(usuario, ct);
        ArgumentNullException.ThrowIfNull(filtro);
        Validator.ValidateObject(filtro, new ValidationContext(filtro), true);
        var consulta = db.cliente.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filtro.Buscar))
        {
            var texto = filtro.Buscar.Trim().ToLower();
            consulta = consulta.Where(c => (c.nombre + " " + (c.apellido ?? "")).ToLower().Contains(texto)
                || (c.nit != null && c.nit.ToLower().Contains(texto))
                || (c.telefono != null && c.telefono.ToLower().Contains(texto)));
        }
        var total = await consulta.CountAsync(ct);
        var items = await consulta.OrderBy(c => c.nombre).ThenBy(c => c.apellido).ThenBy(c => c.id_cliente)
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina).Take(filtro.TamanoPagina)
            .Select(c => new ClienteListadoItem(c.id_cliente, (c.nombre + " " + (c.apellido ?? "")).Trim(), c.nit, c.telefono, c.estado, c.direccion))
            .ToListAsync(ct);
        return new(total, filtro.Pagina, filtro.TamanoPagina, (long)filtro.Pagina * filtro.TamanoPagina < total, items);
    }

    public async Task<ClienteResumen> Resumen(int usuario, int idCliente, CancellationToken ct = default)
    {
        await Administrador(usuario, ct);
        ValidarId(idCliente);
        var cliente = await db.cliente.AsNoTracking().Where(c => c.id_cliente == idCliente)
            .Select(c => new { c.estado, Datos = new ClienteDto { id_Cliente = c.id_cliente, nombre = c.nombre,
                apellido = c.apellido, nit = c.nit, dpi = c.dpi, telefono = c.telefono,
                correo_electronico = c.correo_electronico, direccion = c.direccion } }).SingleOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException("El cliente no existe.");
        // El historial pertenece al cliente, independientemente del cierre de las cajas.
        var totales = await db.ventas.AsNoTracking().Where(v => v.id_cliente == idCliente)
            .GroupBy(v => v.id_cliente).Select(g => new { Cantidad = g.Count(), Total = g.Sum(v => v.total),
                Pagado = g.Sum(v => v.monto_pagado), Saldo = g.Sum(v => v.saldo_pendiente) }).SingleOrDefaultAsync(ct);
        return new(cliente.Datos, cliente.estado, totales?.Cantidad ?? 0, totales?.Total ?? 0,
            totales?.Pagado ?? 0, totales?.Saldo ?? 0);
    }

    public async Task<ClienteVentasPagina> Historial(int usuario, int idCliente, HistorialClientesFiltro filtro, CancellationToken ct = default)
    {
        await Administrador(usuario, ct);
        ValidarId(idCliente);
        ArgumentNullException.ThrowIfNull(filtro);
        Validator.ValidateObject(filtro, new ValidationContext(filtro), true);
        var hoy = DateOnly.FromDateTime(API_SISTEMA.Utilidades.RelojNegocio.Ahora(reloj));
        if (filtro.Fecha is { } fecha && (fecha > hoy || fecha == DateOnly.MaxValue))
            throw new ValidationException("La fecha no puede ser posterior a hoy.");
        var hasta = filtro.Fecha ?? hoy;
        var desde = filtro.Fecha ?? hoy.AddDays(-29);
        var inicio = desde.ToDateTime(TimeOnly.MinValue);
        var finExclusivo = hasta.AddDays(1).ToDateTime(TimeOnly.MinValue);
        if (!await db.cliente.AsNoTracking().AnyAsync(c => c.id_cliente == idCliente, ct))
            throw new KeyNotFoundException("El cliente no existe.");
        var consulta = db.ventas.AsNoTracking().Where(v => v.id_cliente == idCliente
            && v.fecha_venta >= inicio && v.fecha_venta < finExclusivo);
        var total = await consulta.CountAsync(ct);
        var items = await consulta.OrderByDescending(v => v.fecha_venta).ThenByDescending(v => v.id_ventas)
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina).Take(filtro.TamanoPagina)
            .Select(v => new ClienteVentaItem(v.id_ventas, v.fecha_venta, v.id_usuario,
                (v.usuario.nombre + " " + v.usuario.apellido).Trim(), v.total, v.monto_pagado,
                v.saldo_pendiente, v.id_estado_venta, v.EstadoVenta.descripcion ?? "", v.usuario.nombre)).ToListAsync(ct);
        return new(idCliente, total, filtro.Pagina, filtro.TamanoPagina, (long)filtro.Pagina * filtro.TamanoPagina < total, items);
    }

    public async Task<ClienteResumen> Actualizar(int usuario, int idCliente, ActualizarClienteDto datos, CancellationToken ct = default)
    {
        await Administrador(usuario, ct);
        ValidarId(idCliente);
        ArgumentNullException.ThrowIfNull(datos);
        Validator.ValidateObject(datos, new ValidationContext(datos), true);
        var nombre = datos.Nombre.Trim();
        if (nombre.Length == 0) throw new ValidationException("Ingresa el nombre del cliente.");
        static string? Opcional(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        var correo = Opcional(datos.CorreoElectronico);
        if (correo is not null && !new EmailAddressAttribute().IsValid(correo))
            throw new ValidationException("Ingresa un correo electrónico válido.");
        var cliente = await db.cliente.SingleOrDefaultAsync(c => c.id_cliente == idCliente, ct)
            ?? throw new KeyNotFoundException("El cliente no existe.");
        cliente.nombre = nombre;
        cliente.apellido = Opcional(datos.Apellido);
        cliente.nit = Opcional(datos.Nit);
        cliente.dpi = Opcional(datos.Dpi);
        cliente.telefono = Opcional(datos.Telefono);
        cliente.correo_electronico = correo;
        cliente.direccion = Opcional(datos.Direccion);
        await db.SaveChangesAsync(ct);
        return await Resumen(usuario, idCliente, ct);
    }

    private static void ValidarId(int id)
    {
        if (id <= 0) throw new ValidationException("El identificador del cliente debe ser mayor a cero.");
    }
}
