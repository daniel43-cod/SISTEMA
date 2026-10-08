using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Dtos.Mensaje
{
    public class EnviarMensajeDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "El identificador de conversación debe ser positivo.")]
        public int? IdConversacion { get; set; }

        [Required(ErrorMessage = "El mensaje es obligatorio.")]
        [StringLength(2000, ErrorMessage = "El mensaje no puede superar 2000 caracteres.")]
        public required string Mensaje { get; set; }
    }
}
