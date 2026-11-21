using API_SISTEMA.Models;

namespace API_SISTEMA.Services.Fel;

public static class RecuperacionFelReglas
{
    // Superior al timeout de certificación (60 s) y al guardado del resultado (15 s).
    public static readonly TimeSpan Plazo = TimeSpan.FromMinutes(3);

    public static DateTimeOffset Vence(DocumentoFelIntento? intento, DateTimeOffset creado)
        => intento?.procesamiento_vence ?? (intento?.procesamiento_inicio ?? intento?.fecha_creacion ?? creado).Add(Plazo);

    public static bool PuedeRecuperar(string estado, DocumentoFelIntento? intento, DateTimeOffset creado, DateTimeOffset ahora)
        => estado == "POR_CONFIRMAR" || estado == "EN_PROCESO" && Vence(intento, creado) <= ahora;

    public static Guid Tomar(DocumentoFelIntento intento, DateTimeOffset ahora, bool recuperacion)
    {
        if (recuperacion)
        {
            intento.recuperaciones = checked(intento.recuperaciones + 1);
            intento.ultima_recuperacion = ahora;
        }
        intento.procesamiento_inicio = ahora;
        intento.procesamiento_vence = ahora.Add(Plazo);
        intento.procesamiento_token = Guid.NewGuid();
        intento.estado = "EN_PROCESO";
        return intento.procesamiento_token.Value;
    }

    public static bool ConservaControl(string estado, Guid? tokenActual, Guid tokenSolicitud)
        => estado == "EN_PROCESO" && tokenActual == tokenSolicitud;
}
