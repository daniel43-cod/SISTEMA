using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.models
{
    public class Empresa
    {
        [Key]
        public int id_empresa { get; set; }
        [Required]
        public string nombre_empresa { get; set; }
        public string nit { get; set; }

    }
}
