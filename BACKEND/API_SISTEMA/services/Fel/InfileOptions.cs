namespace API_SISTEMA.Services.Fel;

public sealed class InfileOptions
{
    // Habilitar solo después de configurar el emisor y sus credenciales.
    public bool Habilitado { get; set; }
    // Clasifica documentos localmente; el estado del emisor en INFILE determina el Sandbox.
    public string Ambiente { get; set; } = "PRUEBAS";
    public string UsuarioApi { get; set; } = string.Empty;
    public string LlaveApi { get; set; } = string.Empty;
    public string UsuarioFirma { get; set; } = string.Empty;
    public string LlaveFirma { get; set; } = string.Empty;
}
