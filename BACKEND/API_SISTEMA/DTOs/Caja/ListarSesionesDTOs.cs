namespace API_SISTEMA.DTOs.Caja
{
    public class ListarSesionesDTOs
    {
        public int id_sesion_caja { get; set; }
        public int id_caja { get; set; }
        public int id_usuario_apertura { get; set; }
        public int? id_usuario_cierre { get; set; }
        public DateTime fecha_apertura { get; set; }
        public DateTime? fecha_cierre { get; set; }
        public decimal monto_inicial { get; set; }
        public decimal? monto_contado { get; set; }
        public decimal monto_final { get; set; }
        public decimal? diferencia { get; set; }
        public string observacion_apertura { get; set; }
        public string? observacion_cierre { get; set; }

    }
}
