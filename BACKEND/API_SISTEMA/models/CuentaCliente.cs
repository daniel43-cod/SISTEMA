using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_SISTEMA.models
{
    public class CuentaCliente
    {
     [Key]
     [Column("id_cuenta_cliente")]
    public int IdCuentaCliente {get;set;}
    [Column("correo_electronico")]
    public required string CorreoElectronico {get;set;}
    [Column("password_hash")]
    public required string PasswordHash {get;set;}
    [Column("estado")]
    public required bool Estado {get;set;}
    [Column("fecha_creacion")]
    public required DateTime FechaCreacion {get;set;}
    }
}
