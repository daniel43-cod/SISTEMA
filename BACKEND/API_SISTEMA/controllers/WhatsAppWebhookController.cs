using System.Text.Json;
using API_SISTEMA.Services.WhatsApp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace API_SISTEMA.Controllers;

[ApiController, AllowAnonymous, Route("api/whatsapp/webhook")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class WhatsAppWebhookController(WhatsAppWebhookService servicio, IOptions<WhatsAppOptions> options) : ControllerBase
{
    [HttpGet]
    public IActionResult Verificar([FromQuery(Name = "hub.mode")] string? modo,
        [FromQuery(Name = "hub.verify_token")] string? token, [FromQuery(Name = "hub.challenge")] string? challenge)
    {
        if (string.IsNullOrWhiteSpace(options.Value.VerifyToken)) return StatusCode(503);
        if (modo != "subscribe" || !WhatsAppWebhookService.TokensIguales(token, options.Value.VerifyToken) || string.IsNullOrEmpty(challenge)) return StatusCode(403);
        return Content(challenge, "text/plain");
    }

    [HttpPost, RequestSizeLimit(2097152)]
    public async Task<IActionResult> Recibir(CancellationToken ct)
    {
        var config = options.Value;
        if (string.IsNullOrWhiteSpace(config.AppSecret) || string.IsNullOrWhiteSpace(config.BusinessAccountId) || string.IsNullOrWhiteSpace(config.PhoneNumberId))
            return StatusCode(503);
        var firma = Request.Headers["X-Hub-Signature-256"].ToString();
        if (firma.Length != 71) return StatusCode(401);
        using var cuerpo = new MemoryStream();
        var buffer = new byte[8192];
        int leidos;
        while ((leidos = await Request.Body.ReadAsync(buffer, ct)) > 0)
        {
            if (cuerpo.Length + leidos > 2097152) return StatusCode(413);
            cuerpo.Write(buffer, 0, leidos);
        }
        var bytes = cuerpo.ToArray();
        if (!WhatsAppWebhookService.ValidarFirma(bytes, firma, config.AppSecret)) return StatusCode(401);
        try { await servicio.Guardar(bytes, ct); }
        catch (JsonException) { return BadRequest(); }
        catch (DbUpdateException) { return StatusCode(503); }
        return Ok();
    }
}
