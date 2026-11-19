namespace API_SISTEMA.Models;

// Solo conserva estados de entrega. Nunca almacena contenido de chats ni el App Secret.
public sealed class WhatsAppWebhookEvento
{
    public string huella { get; set; } = "";
    public string cuenta { get; set; } = "";
    public string remitente { get; set; } = "";
    public string message_id { get; set; } = "";
    public string estado { get; set; } = "";
    public long timestamp_meta { get; set; }
    public int? codigo_meta { get; set; }
    public DateTime recibido_utc { get; set; }
}
