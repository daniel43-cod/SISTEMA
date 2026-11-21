using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Xml.Linq;
using API_SISTEMA.Models;
using API_SISTEMA.Dtos.Ventas;
using API_SISTEMA.Services.Fel;
using Microsoft.Extensions.Options;

static void Verificar(bool condicion, string mensaje)
{
    if (!condicion) throw new Exception(mensaje);
}
var cfVenta = API_SISTEMA.Services.Ventas.ReceptorVentaReglas.Validar("cf", null, null, 2499.99m);
Verificar(API_SISTEMA.Services.Ventas.ReceptorVentaReglas.ValidarOpcional(null, null, null, 3000m) is null,
    "Registrar una venta sin certificar no debe requerir identificación fiscal, incluso desde Q2,500.");
Verificar(cfVenta.Identificacion == "CF" && cfVenta.Nombre == "CONSUMIDOR FINAL", "No normaliza receptor CF.");
foreach (var datosVenta in new[] { (Tipo: (string?)null, Id: (string?)null, Nombre: (string?)null, Total: 100m),
    (Tipo: "CF", Id: (string?)null, Nombre: (string?)null, Total: 2500m),
    (Tipo: "NIT", Id: "CF", Nombre: "Cliente", Total: 2500m),
    (Tipo: "CUI", Id: "123", Nombre: "Cliente", Total: 100m),
    (Tipo: "NIT", Id: "5203333", Nombre: " ", Total: 100m) })
{
    try
    {
        API_SISTEMA.Services.Ventas.ReceptorVentaReglas.Validar(datosVenta.Tipo, datosVenta.Id, datosVenta.Nombre, datosVenta.Total);
        throw new Exception("Receptor fiscal de venta inválido aceptado.");
    }
    catch (ValidationException) { }
}
Verificar(API_SISTEMA.Services.Ventas.ReceptorVentaReglas.Validar("NIT", "1170142-k", " Cliente ", 2500m).Identificacion == "1170142K",
    "No normaliza el NIT del receptor de la venta.");
var empresa = new Empresa { nit = "5203333", afiliacion_iva = "GEN", codigo_establecimiento = 1,
    razon_social = "Comercio & Ejemplo", nombre_comercial = "DEMO", correo_emisor = "demo@example.com",
    direccion = "Ciudad", codigo_postal = "01001", municipio = "Guatemala", departamento = "Guatemala", pais = "GT" };
var iva20 = API_SISTEMA.Services.Ventas.IvaIncluido.Calcular(20m);
Verificar(iva20.Base == 17.857143m && iva20.Impuesto == 2.142857m && iva20.Base + iva20.Impuesto == 20m,
    "El IVA de Q20 debe conservar seis decimales y el precio final.");
var detalleConIva = API_SISTEMA.Services.Ventas.VentaReglas.Calcular(
    new Productos { IdProducto = 1, Nombre = "Prueba IVA", Stock = 100, PrecioPromedioCompra = 1m },
    new ProductoPresentacion { id_producto_presentacion = 1, precio = 60m, unidades_equivalentes = 1 }, 2, 8m);
Verificar(detalleConIva.subtotal == 112m && detalleConIva.impuesto == 12m,
    "El cálculo de ventas debe aplicar el descuento antes de extraer IVA.");
var receptor = new ReceptorFel("CF", "Cliente <prueba>", new("Ciudad", "01001", "Guatemala", "Guatemala", "GT"));
LineaFacturaFel[] lineas = [new("Producto & <especial>", 2, 60m, 8m, "UND", "B")];
var xml = FacturaXmlGenerator.Generar(empresa, receptor, DateTimeOffset.Parse("2026-11-10T18:00:00+00:00"),
    lineas, [new(1, 1), new(2, 1)], 112m);
var documento = XDocument.Parse(xml);
var validador = new FacturaXsdValidator(Path.Combine(AppContext.BaseDirectory, "Schemas", "Fel"));
// Límite inclusivo: CF solo por debajo de Q2,500.00.
foreach (var monto in new[] { 2500m, 2500.01m })
{
    try
    {
        FacturaXmlGenerator.Generar(empresa, receptor with { Identificacion = " cf " }, DateTimeOffset.UtcNow,
            [new("Bien", 1, monto, 0, "UND", "B")], [new(1, 1)], monto);
        throw new Exception("Se permitió CF en el límite o por encima de él.");
    }
    catch (ValidationException) { }
}
var debajoLimite = FacturaXmlGenerator.Generar(empresa, receptor, DateTimeOffset.UtcNow,
    [new("Bien", 1, 2499.99m, 0, "UND", "B")], [new(1, 1)], 2499.99m);
validador.ValidarAntesDeFirma(debajoLimite);
foreach (var identificado in new[] { receptor with { Identificacion = "5203333" },
    receptor with { Identificacion = "2993930620101", TipoIdentificacion = "CUI" } })
{
    var xmlIdentificado = FacturaXmlGenerator.Generar(empresa, identificado, DateTimeOffset.UtcNow,
        [new("Bien", 1, 2500m, 0, "UND", "B")], [new(1, 1)], 2500m);
    validador.ValidarAntesDeFirma(xmlIdentificado);
    if (identificado.TipoIdentificacion == "CUI")
        Verificar(xmlIdentificado.Contains("TipoEspecial=\"CUI\""), "Falta marcar la identificación CUI.");
}
foreach (var datos in new[] { (Nit: (string?)null, Cui: "123"), (Nit: "CF", Cui: "2993930620101"),
    (Nit: (string?)null, Cui: (string?)null) })
{
    try { IdentificacionReceptorFel.Resolver(datos.Nit, datos.Cui, 2500m); throw new Exception("Identificación inválida aceptada."); }
    catch (ValidationException) { }
}
validador.ValidarAntesDeFirma(xml);
Verificar(!XDocument.Parse(xml).Descendants().Any(e => e.Name.LocalName == "DireccionReceptor"), "CF no debe enviar dirección del receptor.");
var xmlCfSinDireccion = FacturaXmlGenerator.Generar(empresa, receptor with { Direccion = null }, DateTimeOffset.UtcNow,
    lineas, [new(1, 1)], 112m);
validador.ValidarAntesDeFirma(xmlCfSinDireccion);
foreach (var sinDireccion in new[] { receptor with { Identificacion = "5203333", Direccion = null },
    receptor with { Identificacion = "2993930620101", TipoIdentificacion = "CUI", Direccion = null } })
{
    var xmlSinDireccion = FacturaXmlGenerator.Generar(empresa, sinDireccion, DateTimeOffset.UtcNow,
        lineas, [new(1, 1)], 112m);
    validador.ValidarAntesDeFirma(xmlSinDireccion);
    Verificar(!XDocument.Parse(xmlSinDireccion).Descendants().Any(e => e.Name.LocalName == "DireccionReceptor"),
        "NIT/CUI sin dirección debe omitir el bloque completo.");
}
try
{
    FacturaXmlGenerator.Generar(empresa, receptor with { Identificacion = "5203333", Direccion = new("Ciudad", "", "", "", "GT") },
        DateTimeOffset.UtcNow, lineas, [new(1, 1)], 112m);
    throw new Exception("Se aceptó una dirección parcial.");
}
catch (ValidationException) { }
var iva30 = API_SISTEMA.Services.Ventas.IvaIncluido.Calcular(30m).Impuesto;
var xmlSeisDecimales = FacturaXmlGenerator.Generar(empresa, receptor, DateTimeOffset.UtcNow,
    [new("Producto A", 1, 20m, 0m, "UND", "B", iva20.Impuesto), new("Producto B", 1, 30m, 0m, "UND", "B", iva30)],
    [new(1, 1)], 50m, iva20.Impuesto + iva30);
validador.ValidarAntesDeFirma(xmlSeisDecimales);
Verificar(xmlSeisDecimales.Contains(">2.142857<") && xmlSeisDecimales.Contains("TotalMontoImpuesto=\"5.357143\""),
    "El XML debe usar los impuestos guardados sin volver a redondearlos a centavos.");
try
{
    FacturaXmlGenerator.Generar(empresa, receptor, DateTimeOffset.UtcNow,
        [new("Línea inconsistente", 1, 20m, 0m, "UND", "B", 2.14m)], [new(1, 1)], 20m);
    throw new Exception("Se aceptó IVA guardado inconsistente.");
}
catch (ValidationException) { }
try
{
    validador.ValidarAntesDeFirma(xml.Replace("Tipo=\"FACT\"", "Tipo=\"INVALIDO\""));
    throw new Exception("El XSD aceptó un tipo de documento inválido.");
}
catch (ValidationException) { }
XNamespace ns = "http://www.sat.gob.gt/dte/fel/0.2.0";
string Valor(string nombre) => documento.Descendants(ns + nombre).Single().Value;
Verificar(Valor("MontoGravable") == "100.00" && Valor("MontoImpuesto") == "12.00" && Valor("GranTotal") == "112.00", "IVA incluido o descuento incorrecto.");
Verificar(Valor("Descripcion") == "Producto & <especial>", "Escape XML incorrecto.");
Verificar(documento.Descendants(ns + "DatosGenerales").Single().Attribute("FechaHoraEmision")!.Value == "2026-11-10T12:00:00-06:00", "Zona horaria incorrecta.");
try
{
    FacturaXmlGenerator.Generar(empresa, receptor, DateTimeOffset.UtcNow, lineas, [new(1, 1)], 120m);
    throw new Exception("Se aceptó un total inconsistente.");
}
catch (ValidationException) { }
var dto = new CertificarVentaDto { direccion = "Ciudad",
    codigo_postal = "01001", municipio = "Guatemala", departamento = "Guatemala" };
var errores = new List<ValidationResult>();
Verificar(!Validator.TryValidateObject(dto, new ValidationContext(dto), errores, true), "Falta confirmar tratamiento fiscal.");
dto.confirmar_bienes_gravados = true;
Verificar(Validator.TryValidateObject(dto, new ValidationContext(dto), [], true), "DTO válido rechazado.");
var id = Guid.NewGuid();
using var handler = new ServidorSimulado(id);
using var http = new HttpClient(handler);
var cliente = new InfileClient(http, Options.Create(new InfileOptions { Habilitado = true,
    UsuarioApi = "usuario-demo", LlaveApi = "api-demo", UsuarioFirma = "firma-demo", LlaveFirma = "llave-demo" }));
var respuesta = await cliente.EnviarAsync(xml, id);
Verificar(respuesta.Resultado == false && handler.Llamadas == 1, "Respuesta o cantidad de envíos incorrecta.");
var sandbox = await cliente.EnviarAsync(xml, id);
Verificar(sandbox.Resultado == true && sandbox.Numero == "613105939" && sandbox.Serie == "**PRUEBAS**",
    "La respuesta real de Sandbox con número JSON no se interpreta correctamente.");
var intentoNormalizado = new DocumentoFelIntento();
NormalizarRespuestaFel.Aplicar(intentoNormalizado, new(HttpStatusCode.OK,
    """
    {"resultado":false,"fecha":"2026-11-18T16:05:41-06:00","descripcion":"Rechazo de prueba",
     "control_emision":{"Saldo":1980,"Creditos":2000},"cantidad_errores":1,
     "descripcion_errores":[{"fuente":"Firma","categoria":"1","numeral":"2","validacion":"3","mensaje_error":"Firma inválida"}],
     "alertas_infile":true,"descripcion_alertas_infile":["Sandbox"],
     "descripcion_alertas_sat":[{"fuente":"SAT","mensaje_error":"Revisar datos"}]}
    """, false, null, null, null, null));
Verificar(intentoNormalizado.saldo == 1980 && intentoNormalizado.creditos == 2000 &&
    intentoNormalizado.Errores.Single().mensaje == "Firma inválida" && intentoNormalizado.Alertas.Count == 2 &&
    intentoNormalizado.fecha_respuesta!.Value.Offset == TimeSpan.FromHours(-6), "Normalización del rechazo incorrecta.");
var exitoNormalizado = new DocumentoFelIntento();
NormalizarRespuestaFel.Aplicar(exitoNormalizado, new(HttpStatusCode.OK,
    """{"resultado":true,"descripcion":"Certificada","saldo":"950","creditos":"1000","control_emision":{"cupo_diario":1000,"saldo_diario":950},"descripcion_errores":[],"descripcion_alertas_infile":["Sandbox"]}""",
    true, "15B0C102-4DD4-4ED6-8C5C-85B9515720B7", "**PRUEBAS**", "1305759446", null));
Verificar(exitoNormalizado.resultado == true && exitoNormalizado.numero_respuesta == "1305759446" &&
    exitoNormalizado.Errores.Count == 0 && exitoNormalizado.Alertas.Count == 1 && exitoNormalizado.saldo_diario == 950,
    "Normalización de éxito incorrecta.");
var invalidoNormalizado = new DocumentoFelIntento();
NormalizarRespuestaFel.Aplicar(invalidoNormalizado, new(HttpStatusCode.BadGateway, "<html>Error</html>", null, null, null, null, null));
Verificar(invalidoNormalizado.resultado is null && invalidoNormalizado.codigo_http == 502 &&
    invalidoNormalizado.descripcion!.Contains("JSON"), "Respuesta no JSON mal clasificada.");
await ReceptorLookupChecks.Run();
RecuperacionFelChecks.Run();
await VerificarReceptorFelChecks.Run();
CertificadoFelChecks.Run(xml);
Console.WriteLine("FEL: XML, IVA, límite CF, CUI, HTTP simulado y normalización de éxito/rechazo/alertas correctos. Sin llamadas reales.");

sealed class ServidorSimulado(Guid identificador) : HttpMessageHandler
{
    public int Llamadas { get; private set; }
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        Llamadas++;
        if (request.Method != HttpMethod.Post || request.RequestUri!.AbsoluteUri != "https://certificador.feel.com.gt/fel/procesounificado/transaccion/v2/xml" ||
            request.Headers.GetValues("Identificador").Single() != identificador.ToString() ||
            request.Headers.GetValues("LlaveAPI").Single() != "api-demo" ||
            request.Headers.GetValues("LlaveFirma").Single() != "llave-demo" ||
            request.Content!.Headers.ContentType!.MediaType != "application/xml" ||
            !(await request.Content.ReadAsStringAsync(ct)).Contains("GTDocumento"))
            throw new Exception("Contrato HTTP incorrecto.");
        return new(HttpStatusCode.OK) { Content = new StringContent(Llamadas == 1
            ? "{\"resultado\":false,\"descripcion\":\"Rechazo simulado\"}"
            : "{\"resultado\":true,\"numero\":613105939,\"serie\":\"**PRUEBAS**\"}") };
    }
}
