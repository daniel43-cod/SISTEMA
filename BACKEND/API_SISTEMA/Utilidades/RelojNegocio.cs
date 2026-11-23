namespace API_SISTEMA.Utilidades;

/// <summary>Hora civil de Guatemala para las columnas locales existentes del negocio.</summary>
public static class RelojNegocio
{
    private static readonly TimeZoneInfo Zona = TimeZoneInfo.FindSystemTimeZoneById("America/Guatemala");

    // DateTime sin zona: no depende de la configuración horaria del servidor.
    // No usar para campos *_utc, tokens o marcas fiscales DateTimeOffset.
    public static DateTime Ahora(TimeProvider? reloj = null) =>
        TimeZoneInfo.ConvertTime((reloj ?? TimeProvider.System).GetUtcNow(), Zona).DateTime;
}