using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace API_SISTEMA.Services.WhatsApp;

public sealed class WhatsAppOptions
{
    public string AccessToken { get; set; } = "";
    public string PhoneNumberId { get; set; } = "";
    public string BusinessAccountId { get; set; } = "";
    public string ApiVersion { get; set; } = "v27.0";
    public string TemplateName { get; set; } = "envio_de_comprobante";
    public string TemplateLanguage { get; set; } = "es_PA";

    public void Validar()
    {
        if (string.IsNullOrWhiteSpace(AccessToken) || !Regex.IsMatch(PhoneNumberId, @"^[0-9]+$"))
            throw new ValidationException("Configura WhatsApp:AccessToken y WhatsApp:PhoneNumberId en los secretos del backend.");
        if (!Regex.IsMatch(ApiVersion, @"^v[0-9]+\.0$") ||
            !Regex.IsMatch(TemplateName, @"^[a-z0-9_]+$") || !Regex.IsMatch(TemplateLanguage, @"^[a-z]{2}(_[A-Z]{2})?$"))
            throw new ValidationException("Revisa WhatsApp:ApiVersion, TemplateName y TemplateLanguage.");
    }
}
