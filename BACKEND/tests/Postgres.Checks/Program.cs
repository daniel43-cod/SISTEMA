using API_SISTEMA.Data;
using API_SISTEMA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

var options = new DbContextOptionsBuilder<SistemaDbContext>()
    .UseNpgsql("Host=localhost;Database=solo_generacion_sql", o => o.SetPostgresVersion(18, 0)).Options;
using var db = new SistemaDbContext(options);
var sql = db.Database.GenerateCreateScript();
Directory.CreateDirectory(".tmp/postgres-migration");
File.WriteAllText(".tmp/postgres-migration/modelo.sql", sql);
var passed = 0;
void Check(bool ok, string message) { if (!ok) throw new Exception(message); passed++; Console.WriteLine("OK " + message); }
Check(sql.Contains("CREATE TABLE public.ventas") && sql.Contains("CREATE TABLE public.productos"), "Modelo completo genera tablas");
Check(sql.Contains("NULLS NOT DISTINCT") && sql.Contains("WHERE fecha_cierre IS NULL"), "Una sola caja abierta mediante índice único");
Check(sql.Contains("UX_pagos_id_operacion") && sql.Contains("UX_pagos_compra_id_operacion"), "Abonos conservan idempotencia");
Check(sql.Contains("CREATE EXTENSION IF NOT EXISTS citext"), "Identidades insensibles a mayúsculas");
Check(sql.Contains("precio_promedio_compra numeric(18,6)"), "Precisión del costo promedio");
foreach (var entity in db.Model.GetEntityTypes())
foreach (var property in entity.GetProperties())
{
    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
        Check(property.GetColumnType()!.StartsWith("timestamp"), "Fecha mapeada: " + entity.ClrType.Name + "." + property.Name);
}
Check(db.Model.FindEntityType(typeof(SesionUsuario))!.FindProperty(nameof(SesionUsuario.RowVersion))!.IsConcurrencyToken, "Concurrencia de sesiones");
Check(db.Model.FindEntityType(typeof(DocumentoFel))!.FindProperty(nameof(DocumentoFel.version))!.GetColumnName() == "xmin", "Concurrencia FEL usa xmin");
Console.WriteLine($"{passed} comprobaciones del modelo PostgreSQL aprobadas, sin abrir conexiones.");
if (Environment.GetEnvironmentVariable("POSTGRES_TEST_CONNECTION") is { Length: > 0 } connection)
    try { await PostgreSqlIntegration.Run(connection); } catch (Exception e) { Console.Error.WriteLine(e); Environment.ExitCode = 1; }
