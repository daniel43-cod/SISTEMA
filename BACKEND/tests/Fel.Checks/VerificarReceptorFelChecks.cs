using System.ComponentModel.DataAnnotations;
using API_SISTEMA.Services.Fel;
using API_SISTEMA.Utilidades;

internal static class VerificarReceptorFelChecks
{
    public static async Task Run()
    {
        var consulta = new ConsultaSimulada();
        var servicio = new VerificarReceptorFelService(consulta);
        var guardado = await servicio.Verificar(null, null, "NIT", "1170142-k", 100m, default);
        Check(consulta.Llamadas == 1 && guardado.Nombre == "NOMBRE OFICIAL" && guardado.Identificacion == "1170142K",
            "Omitir el tipo debe consultar el NIT guardado; no aceptar nombre manual.");
        var cui = await servicio.Verificar(null, null, "CUI", "2993930620101", 3000m, default);
        Check(consulta.Llamadas == 2 && cui.Nombre == "NOMBRE OFICIAL" && cui.Tipo == "CUI", "CUI guardado también debe consultarse.");
        var explicito = await servicio.Verificar("NIT", "5203333", "NIT", "1170142K", 3000m, default);
        Check(consulta.UltimoDocumento == "5203333" && explicito.Identificacion == "5203333", "Verificar documento explícito efectivo.");
        var antesCf = consulta.Llamadas;
        var cf = await servicio.Verificar(null, null, "CF", "CF", 2499.99m, default);
        Check(cf.Nombre == "CONSUMIDOR FINAL" && consulta.Llamadas == antesCf, "CF no consume consulta externa.");
        try { await servicio.Verificar(null, null, "CF", "CF", 2500m, default); throw new Exception("CF inválido aceptado."); }
        catch (ValidationException) { }
        consulta.Encontrado = false;
        try { await servicio.Verificar(null, null, "NIT", "1170142K", 100m, default); throw new Exception("Se certificó receptor sin verificar."); }
        catch (ValidationException) { }
        VerificarReceptorFelService.Confirmar(guardado, null, null, "NIT", "1170142-K", 100m);
        try { VerificarReceptorFelService.Confirmar(guardado, null, null, "NIT", "5203333", 100m); throw new Exception("Cambio de documento concurrente aceptado."); }
        catch (VentaConflictoException) { }
        var invalidos = new (string? tipo, string? documento, string? guardado, string? id, decimal total)[]
        {
            (null, null, null, null, 100m), ("", null, "CF", "CF", 100m), ("DPI", "123", null, null, 100m),
            (null, "5203333", "NIT", "1170142K", 100m), ("CF", "5203333", null, null, 100m),
            ("CF", null, null, null, 2500m), (null, null, "CF", "CF", 2500.01m),
            ("NIT", null, "NIT", "5203333", 100m), ("NIT", "CF", null, null, 100m),
            ("NIT", "string", null, null, 100m), ("NIT", new string('1', 21), null, null, 100m),
            ("CUI", "123", null, null, 100m), ("CUI", "299393062010X", null, null, 100m),
            ("CUI", "29939306201011", null, null, 100m), ("CUI", "null", null, null, 100m)
        };
        foreach (var caso in invalidos)
        {
            var antes = consulta.Llamadas;
            await Falla<ValidationException>(() => servicio.Verificar(caso.tipo, caso.documento, caso.guardado, caso.id, caso.total, default));
            Check(consulta.Llamadas == antes, "Entrada inválida no debe consultar INFILE.");
        }
        foreach (var respuesta in new ConsultaReceptorResultado[]
        {
            new(false, "NIT", "5203333", "NOMBRE MANUAL", null, null),
            new(true, "CUI", "5203333", "NOMBRE", "ACTIVO", null),
            new(true, "NIT", "1170142K", "NOMBRE", null, null),
            new(true, "NIT", "5203333", null, null, null),
            new(true, "NIT", "5203333", "  ", null, null),
            new(true, "NIT", "5203333", new string('X', 251), null, null),
            new(true, "NIT", "5203333", "NOMBRE\nFALSO", null, null)
        })
        {
            consulta.Respuesta = respuesta;
            await Falla<ValidationException>(() => servicio.Verificar(null, null, "NIT", "5203333", 100m, default));
        }
        foreach (var estado in new string?[] { null, "FALLECIDO", "NO_ENCONTRADO", "" })
        {
            consulta.Respuesta = new(true, "CUI", "2993930620101", "NOMBRE", estado, null);
            await Falla<ValidationException>(() => servicio.Verificar(null, null, "CUI", "2993930620101", 100m, default));
        }
        consulta.Respuesta = null;
        consulta.Error = new ConsultaReceptorNoDisponibleException();
        await Falla<ConsultaReceptorNoDisponibleException>(() => servicio.Verificar(null, null, "NIT", "5203333", 100m, default));
        consulta.Error = null;
        consulta.Encontrado = true;
        using var cancelacion = new CancellationTokenSource();
        cancelacion.Cancel();
        await Falla<OperationCanceledException>(() => servicio.Verificar("NIT", "5203333", null, null, 100m, cancelacion.Token));
        await Falla<VentaConflictoException>(() => Task.Run(() => VerificarReceptorFelService.Confirmar(guardado, null, null, "CF", "CF", 100m)));
        var cfVerificado = await servicio.Verificar("CF", null, null, null, 2499.99m, default);
        await Falla<ValidationException>(() => Task.Run(() => VerificarReceptorFelService.Confirmar(cfVerificado, "CF", null, null, null, 2500m)));
        try { VerificarReceptorFelService.Confirmar(null, null, null, "NIT", "1170142K", 100m); throw new Exception("No se exigió consulta previa."); }
        catch (VentaConflictoException) { }
        Console.WriteLine("Receptor: omisión de tipo, documento guardado/explícito, nombre oficial, CF y cambios concurrentes verificados.");
    }
    private static void Check(bool condicion, string mensaje) { if (!condicion) throw new Exception(mensaje); }
    private static async Task Falla<T>(Func<Task> accion) where T : Exception
    {
        try { await accion(); }
        catch (T) { return; }
        throw new Exception("Se esperaba " + typeof(T).Name);
    }
    private sealed class ConsultaSimulada : IConsultaReceptorFel
    {
        public int Llamadas;
        public string? UltimoDocumento;
        public bool Encontrado = true;
        public ConsultaReceptorResultado? Respuesta;
        public Exception? Error;
        public Task<ConsultaReceptorResultado> Consultar(string tipo, string identificacion, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            Llamadas++;
            UltimoDocumento = identificacion;
            if (Error is not null) throw Error;
            return Task.FromResult(Respuesta ?? new ConsultaReceptorResultado(Encontrado, tipo, identificacion,
                Encontrado ? "NOMBRE OFICIAL" : null, "ACTIVO", null));
        }
    }
}
