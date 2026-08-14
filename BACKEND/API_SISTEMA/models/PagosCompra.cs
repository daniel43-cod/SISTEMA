using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.models
{
    public class PagosCompra
    {
        [Key]
        public int id_pagos_compra { get; set; }
        [Required]
        public int id_compra { get; set; }
        [Required]
        public int id_usuario { get; set; }
        public string observacion { get; set; }
        [Required]
        public DateTime fecha_pago { get; set; }
        [Required]
        public decimal? monto { get; set; }
        public int id_sesion_caja { get; set; }
        public SesionCaja sesioncaja { get; set; }
    }
}
