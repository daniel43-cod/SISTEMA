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
        [MaxLength(150)]
        public string? ubicacion { get; set; }
        [MaxLength(250)]
        public string? direccion { get; set; }
        [MaxLength(150)]
        public string? telefonos { get; set; }

        [MaxLength(250)]
        public string? razon_social { get; set; }
        [MaxLength(250)]
        public string? nombre_comercial { get; set; }
        [MaxLength(10)]
        public string? afiliacion_iva { get; set; }
        public int? codigo_establecimiento { get; set; }
        [MaxLength(254)]
        public string? correo_emisor { get; set; }
        [MaxLength(10)]
        public string? codigo_postal { get; set; }
        [MaxLength(100)]
        public string? municipio { get; set; }
        [MaxLength(100)]
        public string? departamento { get; set; }
        [MaxLength(2)]
        public string? pais { get; set; }

    }
}
