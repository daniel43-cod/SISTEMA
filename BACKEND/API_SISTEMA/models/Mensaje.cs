using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_SISTEMA.models
{
    public class Mensaje
    {
        [Key]
      [Column("id_mensaje")]
      public int IdMensaje {get; set;}
      [Column("id_conversacion")]
      public int IdConversacion {get; set;}
      [Column("mensaje")]
      public required string MensajeRecibido {get; set;}
      [Column("respuesta")]
      public  string? Respuesta {get; set;}

      public Conversacion Conversacion {get;set;} = null!;

    }
}
