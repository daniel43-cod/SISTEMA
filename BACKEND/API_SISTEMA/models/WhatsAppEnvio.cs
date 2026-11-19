namespace API_SISTEMA.Models;

public sealed class WhatsAppEnvio
{
    public Guid id_envio { get; set; }
    public int id_venta { get; set; }
    public int id_empresa { get; set; }
    public int id_usuario { get; set; }
    public string telefono { get; set; } = "";
    public string plantilla { get; set; } = "";
    public string idioma { get; set; } = "";
    public string remitente { get; set; } = "";
    public DateTime creado_utc { get; set; }
    public List<WhatsAppEnvioPagina> paginas { get; set; } = [];
}

public sealed class WhatsAppEnvioPagina
{
    public Guid id_envio { get; set; }
    public int pagina { get; set; }
    public string estado { get; set; } = "no_intentado";
    public string? message_id { get; set; }
    public string? etapa { get; set; }
    public int? codigo_meta { get; set; }
    public DateTime? actualizado_utc { get; set; }
}

public sealed class WhatsAppEnvioIntento
{
    public long id_intento { get; set; }
    public Guid id_envio { get; set; }
    public int pagina { get; set; }
    public int id_usuario { get; set; }
    public DateTime inicio_utc { get; set; }
    public DateTime? fin_utc { get; set; }
    public string estado { get; set; } = "subiendo";
    public string? message_id { get; set; }
    public string? etapa { get; set; }
    public int? codigo_meta { get; set; }
}
