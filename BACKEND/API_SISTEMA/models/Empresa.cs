using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Models
{
    public class Empresa
    {
        [Key]
        public int id_empresa { get; set; }
        [Required]
        public  string nombre_empresa { get; set; } = string.Empty;
        public string? nit { get; set; }

    }
}
