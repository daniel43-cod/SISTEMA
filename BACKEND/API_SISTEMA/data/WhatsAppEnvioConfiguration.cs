using API_SISTEMA.Models;
using Microsoft.EntityFrameworkCore;

namespace API_SISTEMA.Data;

public static class WhatsAppEnvioConfiguration
{
    public static void Configurar(ModelBuilder model)
    {
        model.Entity<WhatsAppWebhookEvento>(e => {
            e.ToTable("whatsapp_webhook_evento", "public");
            e.HasKey(x => x.huella);
            e.Property(x => x.huella).HasMaxLength(64).IsUnicode(false);
            e.Property(x => x.cuenta).HasMaxLength(30).IsUnicode(false);
            e.Property(x => x.remitente).HasMaxLength(30).IsUnicode(false);
            e.Property(x => x.message_id).HasMaxLength(512).IsUnicode(false);
            e.Property(x => x.estado).HasMaxLength(15).IsUnicode(false);
            e.HasIndex(x => x.message_id).HasDatabaseName("IX_whatsapp_webhook_message_id");
        });
        model.Entity<WhatsAppEnvio>(e => {
            e.ToTable("whatsapp_envio", "public");
            e.HasKey(x => x.id_envio);
            e.Property(x => x.id_envio).ValueGeneratedNever();
            e.HasIndex(x => new { x.id_venta, x.id_empresa }).HasDatabaseName("IX_whatsapp_envio_venta_empresa");
            e.Property(x => x.telefono).HasMaxLength(15).IsUnicode(false);
            e.Property(x => x.plantilla).HasMaxLength(512).IsUnicode(false);
            e.Property(x => x.idioma).HasMaxLength(10).IsUnicode(false);
            e.Property(x => x.remitente).HasMaxLength(30).IsUnicode(false);
            e.HasMany(x => x.paginas).WithOne().HasForeignKey(x => x.id_envio).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Ventas>().WithMany().HasForeignKey(x => x.id_venta).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Empresa>().WithMany().HasForeignKey(x => x.id_empresa).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Usuario>().WithMany().HasForeignKey(x => x.id_usuario).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<WhatsAppEnvioPagina>(p => {
            p.ToTable("whatsapp_envio_pagina", "public");
            p.HasKey(x => new { x.id_envio, x.pagina });
            p.Property(x => x.estado).HasMaxLength(20).IsUnicode(false);
            p.Property(x => x.message_id).HasMaxLength(512).IsUnicode(false);
            p.Property(x => x.etapa).HasMaxLength(20).IsUnicode(false);
        });
        model.Entity<WhatsAppEnvioIntento>(i => {
            i.ToTable("whatsapp_envio_intento", "public");
            i.HasKey(x => x.id_intento);
            i.Property(x => x.estado).HasMaxLength(20).IsUnicode(false);
            i.Property(x => x.message_id).HasMaxLength(512).IsUnicode(false);
            i.Property(x => x.etapa).HasMaxLength(20).IsUnicode(false);
            i.HasIndex(x => x.message_id).HasFilter("message_id IS NOT NULL");
            i.HasOne<WhatsAppEnvioPagina>().WithMany().HasForeignKey(x => new { x.id_envio, x.pagina }).OnDelete(DeleteBehavior.Restrict);
            i.HasOne<Usuario>().WithMany().HasForeignKey(x => x.id_usuario).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
