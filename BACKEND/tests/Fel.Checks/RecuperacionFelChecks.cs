using API_SISTEMA.Models;
using API_SISTEMA.Services.Fel;

internal static class RecuperacionFelChecks
{
    public static void Run()
    {
        var ahora = DateTimeOffset.Parse("2026-11-19T18:00:00-06:00");
        var identificador = Guid.NewGuid();
        var intento = new DocumentoFelIntento { identificador = identificador, xml_enviado = "<original />", fecha_creacion = ahora };
        var primera = RecuperacionFelReglas.Tomar(intento, ahora, false);
        Check(intento.recuperaciones == 0 && intento.procesamiento_vence == ahora.AddMinutes(3), "Toma inicial con plazo");
        Check(!RecuperacionFelReglas.PuedeRecuperar("EN_PROCESO", intento, ahora, ahora.AddMinutes(3).AddTicks(-1)), "No recuperar antes de vencer");
        Check(RecuperacionFelReglas.PuedeRecuperar("EN_PROCESO", intento, ahora, ahora.AddMinutes(3)), "Recuperar desde el vencimiento exacto");
        Check(RecuperacionFelReglas.PuedeRecuperar("POR_CONFIRMAR", intento, ahora, ahora), "Por confirmar permite reconciliación inmediata");
        var segunda = RecuperacionFelReglas.Tomar(intento, ahora.AddMinutes(3), true);
        Check(segunda != primera && intento.recuperaciones == 1 && intento.ultima_recuperacion == ahora.AddMinutes(3), "Recuperación cambia control y cuenta");
        Check(intento.identificador == identificador && intento.xml_enviado == "<original />", "Nunca cambia identificador INFILE ni XML");
        Check(!RecuperacionFelReglas.ConservaControl("EN_PROCESO", segunda, primera), "Respuesta antigua no tiene control");
        Check(RecuperacionFelReglas.ConservaControl("EN_PROCESO", segunda, segunda), "Solo puede guardar el propietario vigente");
        Check(!RecuperacionFelReglas.ConservaControl("CERTIFICADA", segunda, segunda), "No sobrescribir estado terminal");
        foreach (var estado in new[] { "CERTIFICADA", "RECHAZADA", "PENDIENTE" })
            Check(!RecuperacionFelReglas.PuedeRecuperar(estado, intento, ahora, ahora.AddDays(1)), "No recuperar estado " + estado);
        Check(!RecuperacionFelReglas.PuedeRecuperar("EN_PROCESO", null, ahora, ahora.AddMinutes(2)), "Legado reciente conserva margen");
        Check(RecuperacionFelReglas.PuedeRecuperar("EN_PROCESO", null, ahora, ahora.AddMinutes(3)), "Legado antiguo puede reconciliarse");
        Console.WriteLine("Recuperación FEL: plazo, límites, legado, control exclusivo y conservación del XML/identificador verificados.");
    }
    private static void Check(bool condicion, string mensaje) { if (!condicion) throw new Exception(mensaje); }
}
