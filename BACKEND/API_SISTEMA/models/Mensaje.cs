using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_SISTEMA.Models
{
    public class Mensaje
    {
        [Key]
      [Column("id_mensaje")]
      public int IdMensaje {get; set;}
      [Column("id_conversacion")]
      public int IdConversacion {get; set;}
      [Column("mensaje", TypeName = "text")]
      public required string MensajeRecibido {get; set;}
      [Column("respuesta", TypeName = "text")]
      public  string? Respuesta {get; set;}

      public Conversacion Conversacion {get;set;} = null!;

    }
}
