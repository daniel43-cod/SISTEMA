using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_SISTEMA.models
{
    public class Conversacion
    {
     [Key]
     [Column("id_conversacion")]
     public int IdConversacion {get;set;}
     [Column("id_usuario")]
     public int? IdUsuario {get;set;}
     [Column("id_cuenta_cliente")]
     public int IdCuentaCliente {get;set;}

    public Usuario? Usuario {get;set;}
    public CuentaCliente CuentaCliente {get;set;} = null!;

    }
}
