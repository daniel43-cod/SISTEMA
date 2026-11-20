using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Ventas;
using API_SISTEMA.Models;
using API_SISTEMA.Services.Permisos;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace API_SISTEMA.Services.Fel;

public sealed record EstadoFacturaFel(int id_documento_fel, int id_venta, string ambiente, string estado,
    string? uuid, string? serie, string? numero, DateTimeOffset? fecha_certificacion, string? pdf)
{
    public long? id_intento { get; init; }
    public string? descripcion { get; init; }
    public IReadOnlyList<MensajeFel> errores { get; init; } = [];
    public IReadOnlyList<MensajeFel> alertas { get; init; } = [];
}
public sealed record MensajeFel(string? fuente, string? categoria, string? numeral, string? validacion, string? mensaje);

public sealed class CertificarVentaService(SistemaDbContext db, InfileClient infile,
    IOptions<InfileOptions> options, PermisoUsuarioService permisos, FacturaXsdValidator validador,
    ConsultarReceptorInfileService consultaReceptor)
{
    private static EstadoFacturaFel Resumen(DocumentoFel d) => new(d.id_documento_fel, d.id_venta, d.ambiente,
        d.estado, d.uuid, d.serie, d.numero, d.fecha_certificacion,
        d.estado == "CERTIFICADA" ? $"https://report.feel.com.gt/ingfacereport/ingfacereport_documento?uuid={Uri.EscapeDataString(d.uuid!)}" : null);

    private async Task<EstadoFacturaFel> ResumenConMensajes(DocumentoFel documento, CancellationToken ct)
    {
        var intento = await db.DocumentosFelIntentos.AsNoTracking().Include(i => i.Errores).Include(i => i.Alertas)
            .Where(i => i.id_documento_fel == documento.id_documento_fel).OrderByDescending(i => i.id_intento).FirstOrDefaultAsync(ct);
        return Resumen(documento) with
        {
            id_intento = intento?.id_intento, descripcion = intento?.descripcion,
            errores = intento?.Errores.OrderBy(e => e.orden).Select(e => new MensajeFel(e.fuente, e.categoria, e.numeral, e.validacion, e.mensaje)).ToArray() ?? [],
            alertas = intento?.Alertas.OrderBy(a => a.origen).ThenBy(a => a.orden).Select(a => new MensajeFel(a.origen, a.categoria, a.numeral, a.validacion, a.mensaje)).ToArray() ?? []
        };
    }

    public async Task<EstadoFacturaFel?> Consultar(int idVenta, int usuario, CancellationToken ct)
    {
        await Autorizar(usuario);
        if (!await db.ventas.AnyAsync(v => v.id_ventas == idVenta, ct)) throw new KeyNotFoundException("La venta no existe.");
        var documento = await db.DocumentosFel.AsNoTracking()
            .SingleOrDefaultAsync(d => d.id_venta == idVenta && d.ambiente == options.Value.Ambiente, ct);
        return documento is null ? null : await ResumenConMensajes(documento, ct);
    }

    private async Task Autorizar(int usuario)
    {
        if (!await permisos.TienePermiso(usuario, Utilidades.Permisos.Vender))
            throw new UnauthorizedAccessException("No tienes permiso para certificar ventas.");
    }

    public async Task<object> Preparar(int idVenta, int usuario, CancellationToken ct)
    {
        await Autorizar(usuario);
        return await db.ventas.AsNoTracking().Where(v => v.id_ventas == idVenta)
            .Select(v => new { v.total, v.tipo_identificacion_receptor, v.identificacion_receptor, v.nombre_receptor })
            .SingleOrDefaultAsync(ct) ?? throw new KeyNotFoundException("La venta no existe.");
    }

    public async Task<EstadoFacturaFel> Certificar(int idVenta, CertificarVentaDto dto, int usuario, CancellationToken ct)
    {
        await Autorizar(usuario);
        var config = options.Value;
        if (!config.Habilitado) throw new ValidationException("La integración FEL está deshabilitada.");
        // Habilitar producción después de validar XSD, reglas fiscales y pruebas integrales.
        if (config.Ambiente != "PRUEBAS") throw new ValidationException("Esta versión de certificación está habilitada únicamente para pruebas.");
        // Consultar fuera de la transacción SQL. En reconciliación se respeta el XML anterior.
        var estadoAnterior = await db.DocumentosFel.AsNoTracking()
            .Where(d => d.id_venta == idVenta && d.ambiente == config.Ambiente).Select(d => d.estado).SingleOrDefaultAsync(ct);
        string? nombreConsultado = null;
        if ((estadoAnterior is null or "RECHAZADA") && (dto.tipo_identificacion_receptor is "NIT" or "CUI"))
        {
            var consulta = await consultaReceptor.Consultar(dto.tipo_identificacion_receptor, dto.identificacion_receptor ?? "", ct);
            if (!consulta.resultado) throw new ValidationException(consulta.descripcion ?? "No se pudo validar al receptor.");
            nombreConsultado = consulta.nombre;
        }
        DocumentoFel documento;
        DocumentoFelIntento intento;
        await using (var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct))
        {
            // Comparte el bloqueo de la venta con su edición; no mantiene SQL abierto durante HTTP.
            var venta = await db.ventas.FromSqlInterpolated($"SELECT * FROM dbo.ventas WITH (UPDLOCK, HOLDLOCK) WHERE id_ventas = {idVenta}")
                .Include(v => v.DetalleVentas).ThenInclude(d => d.Producto)
                .SingleOrDefaultAsync(ct) ?? throw new KeyNotFoundException("La venta no existe.");
            var existente = await db.DocumentosFel.SingleOrDefaultAsync(d => d.id_venta == idVenta && d.ambiente == config.Ambiente, ct);
            if (existente is not null)
            {
                if (existente.estado == "CERTIFICADA") return await ResumenConMensajes(existente, ct);
                if (existente.estado is not ("POR_CONFIRMAR" or "RECHAZADA"))
                    throw new VentaConflictoException("El documento ya está registrado. Consulta su estado antes de continuar.");
                // Ignorar cambios del cuerpo en un reintento: se conserva el XML original.
                documento = existente;
            }
            if (existente is null || existente.estado == "RECHAZADA")
            {
                // No sobrescribir un rechazo histórico si no fue migrado aún.
                if (existente is not null && !await db.DocumentosFelIntentos.AnyAsync(i => i.identificador == existente.identificador, ct))
                    throw new VentaConflictoException("Falta migrar el intento rechazado. Ejecuta el script de normalización antes de corregirlo.");
                Validator.ValidateObject(dto, new ValidationContext(dto), true);
                if (!dto.confirmar_bienes_gravados) throw new ValidationException("Confirma la clasificación fiscal de los productos.");
                var empresas = await db.empresa.AsNoTracking().Take(2).ToListAsync(ct);
                if (empresas.Count != 1) throw new ValidationException("Debe existir exactamente una empresa configurada.");
                var fecha = DateTimeOffset.UtcNow;
                var identidad = API_SISTEMA.Services.Ventas.ReceptorVentaReglas.Validar(
                    dto.tipo_identificacion_receptor ?? venta.tipo_identificacion_receptor,
                    dto.tipo_identificacion_receptor is null ? venta.identificacion_receptor : dto.identificacion_receptor,
                    dto.tipo_identificacion_receptor is null ? venta.nombre_receptor : nombreConsultado, venta.total);
                venta.tipo_identificacion_receptor = identidad.Tipo;
                venta.identificacion_receptor = identidad.Identificacion;
                venta.nombre_receptor = identidad.Nombre;
                var tieneDireccion = new[] { dto.direccion, dto.codigo_postal, dto.municipio, dto.departamento }
                    .Any(valor => !string.IsNullOrWhiteSpace(valor));
                var receptor = new ReceptorFel(identidad.Identificacion, identidad.Nombre,
                    identidad.Tipo == "CF" || !tieneDireccion ? null : new(dto.direccion!, dto.codigo_postal!, dto.municipio!, dto.departamento!, dto.pais!), dto.correo_receptor,
                    identidad.Tipo == "CUI" ? "CUI" : "NIT");
                var lineas = venta.DetalleVentas.OrderBy(d => d.id_detalle_venta).Select(d =>
                    new LineaFacturaFel(d.Producto.Nombre, d.cantidad, d.precio, d.descuento ?? 0m, "UND", "B", d.impuesto)).ToArray();
                documento = existente ?? new DocumentoFel { id_venta = idVenta, ambiente = config.Ambiente };
                documento.id_usuario = usuario;
                documento.identificador = Guid.NewGuid();
                documento.fecha_emision = fecha;
                documento.xml_enviado = FacturaXmlGenerator.Generar(empresas[0], receptor, fecha, lineas, config.Frases, venta.total,
                    venta.DetalleVentas.All(d => d.impuesto.HasValue) ? venta.impuesto : null);
                if (existente is null) db.DocumentosFel.Add(documento);
            }
            else documento = existente!;
            validador.ValidarAntesDeFirma(documento.xml_enviado);
            documento.estado = "EN_PROCESO";
            await db.SaveChangesAsync(ct);
            intento = await db.DocumentosFelIntentos.Include(i => i.Errores).Include(i => i.Alertas)
                .SingleOrDefaultAsync(i => i.identificador == documento.identificador, ct)
                ?? new DocumentoFelIntento { id_documento_fel = documento.id_documento_fel, id_usuario = usuario,
                    identificador = documento.identificador, fecha_emision = documento.fecha_emision, xml_enviado = documento.xml_enviado };
            if (intento.id_intento == 0) db.DocumentosFelIntentos.Add(intento);
            intento.estado = "EN_PROCESO";
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }

        InfileRespuesta? respuestaRecibida = null;
        try
        {
            var respuesta = await infile.EnviarAsync(documento.xml_enviado, documento.identificador, ct);
            respuestaRecibida = respuesta;
            documento.estado = "POR_CONFIRMAR";
            if ((int)respuesta.EstadoHttp is >= 200 and < 300)
            {
                if (respuesta.Resultado == false) documento.estado = "RECHAZADA";
                else if (respuesta.Resultado == true) AplicarCertificacion(documento, respuesta);
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or FormatException or XmlException)
        {
            documento.estado = "POR_CONFIRMAR";
        }
        // Persistir el resultado aunque el navegador haya cancelado la solicitud.
        using var guardar = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        await using var resultadoTx = await db.Database.BeginTransactionAsync(guardar.Token);
        if (respuestaRecibida is not null)
        {
            // Reconciliar el mismo identificador actualiza sus mensajes; no crea otro intento.
            db.RemoveRange(intento.Errores);
            db.RemoveRange(intento.Alertas);
            intento.Errores.Clear();
            intento.Alertas.Clear();
            await db.SaveChangesAsync(guardar.Token);
            NormalizarRespuestaFel.Aplicar(intento, respuestaRecibida);
        }
        else intento.descripcion = "No se recibió una respuesta confirmada de INFILE. Reintenta con el mismo identificador.";
        intento.estado = documento.estado;
        intento.xml_certificado = documento.xml_certificado;
        await db.SaveChangesAsync(guardar.Token);
        await resultadoTx.CommitAsync(guardar.Token);
        return await ResumenConMensajes(documento, guardar.Token);
    }

    private static void AplicarCertificacion(DocumentoFel documento, InfileRespuesta respuesta)
    {
        if (!Guid.TryParse(respuesta.Uuid, out _) || respuesta.Serie?.Length is not (>= 1 and <= 20) ||
            respuesta.Numero?.Length is not (>= 1 and <= 30) || string.IsNullOrWhiteSpace(respuesta.XmlCertificadoBase64)) return;
        var xml = Encoding.UTF8.GetString(Convert.FromBase64String(respuesta.XmlCertificadoBase64));
        using var reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
        var certificado = XDocument.Load(reader);
        XNamespace ns = "http://www.sat.gob.gt/dte/fel/0.2.0";
        var autorizacion = certificado.Descendants(ns + "NumeroAutorizacion").SingleOrDefault();
        var fecha = certificado.Descendants(ns + "FechaHoraCertificacion").SingleOrDefault()?.Value;
        if (!string.Equals(autorizacion?.Value.Trim(), respuesta.Uuid, StringComparison.OrdinalIgnoreCase) ||
            !DateTimeOffset.TryParse(fecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaCertificada)) return;
        documento.uuid = respuesta.Uuid;
        documento.serie = respuesta.Serie;
        documento.numero = respuesta.Numero;
        documento.xml_certificado = xml;
        documento.fecha_certificacion = fechaCertificada;
        documento.estado = "CERTIFICADA";
    }
}
