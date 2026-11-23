using System.ComponentModel.DataAnnotations;
using System.Data;
using API_SISTEMA.Data;
using API_SISTEMA.Dtos.Login;
using API_SISTEMA.Models;
using API_SISTEMA.Utilidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// Inicialización explícita y única. No migra ni borra tablas; no cambia cuentas existentes.
try
{
    var config = new ConfigurationBuilder().AddUserSecrets<SistemaDbContext>(optional: true).AddEnvironmentVariables().Build();
    string Requerido(string campo) => config["Bootstrap:" + campo] is { Length: > 0 } value ? value.Trim() :
        throw new InvalidOperationException("Configura Bootstrap:" + campo + " antes de inicializar.");
    await using var db = new SistemaDbContextFactory().CreateDbContext(args);
    if ((await db.Database.GetPendingMigrationsAsync()).Any()) throw new InvalidOperationException("Primero aplica las migraciones y los catálogos de datos-iniciales.sql.");
    await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
    await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(682719043)");
    if (await db.usuarios.AnyAsync()) throw new InvalidOperationException("Ya existen usuarios: la inicialización no se puede repetir.");
    var rol = await db.rols.SingleOrDefaultAsync(r => r.nombre == Roles.Administrador && r.estado) ??
        throw new InvalidOperationException("Aplica docs/sql/datos-iniciales.sql antes de inicializar.");
    var dto = new CrearCuentaDto {
        id_rol = rol.id_rol, usuario = Requerido("Usuario"), nombre = Requerido("Nombre"), apellido = Requerido("Apellido"),
        telefono = Requerido("Telefono"), corre_electronico = config["Bootstrap:Correo"],
        password = config["Bootstrap:Password"] ?? throw new InvalidOperationException("Configura Bootstrap:Password.")
    };
    Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);
    if (dto.usuario.Any(char.IsControl) || dto.nombre.Any(char.IsControl) || dto.apellido.Any(char.IsControl) ||
        dto.telefono.Any(c => !char.IsAsciiDigit(c) && c != '+' && c != '-' && c != ' ' && c != '(' && c != ')') || !dto.telefono.Any(char.IsAsciiDigit))
        throw new ValidationException("Revisa nombre, usuario y teléfono.");
    var nombreCaja = Requerido("NombreCaja");
    if (nombreCaja.Length > 100) throw new ValidationException("NombreCaja admite hasta 100 caracteres.");
    var usuario = new Usuario {
        id_rol = rol.id_rol, nombre = dto.nombre, apellido = dto.apellido, usuario = dto.usuario,
        correo = dto.corre_electronico, telefono = dto.telefono, password = BCrypt.Net.BCrypt.HashPassword(dto.password),
        estado = true, fecha_Creacion = RelojNegocio.Ahora()
    };
    db.usuarios.Add(usuario);
    if (!await db.caja.AnyAsync()) db.caja.Add(new Caja { nombre_caja = nombreCaja, estado = true });
    await db.SaveChangesAsync();
    db.AuditoriaEventos.Add(new AuditoriaEvento {
        FechaUtc = DateTime.UtcNow, IdUsuario = usuario.id_usuario, UsuarioResponsable = usuario.usuario,
        Accion = "SISTEMA_INICIALIZADO", Entidad = "usuario", IdRegistro = usuario.id_usuario.ToString(),
        Resultado = "EXITOSO", Origen = "SISTEMA"
    });
    await db.SaveChangesAsync(); await tx.CommitAsync();
    Console.WriteLine("Administrador inicial y caja creados. Elimina Bootstrap:Password de la configuración.");
}
catch (ValidationException e) { Console.Error.WriteLine(e.Message); Environment.ExitCode = 1; }
catch (InvalidOperationException e) { Console.Error.WriteLine(e.Message); Environment.ExitCode = 1; }
catch { Console.Error.WriteLine("No se pudo completar la inicialización. Revisa conexión y permisos; no se muestran credenciales."); Environment.ExitCode = 1; }