namespace API_SISTEMA.DTOs.Gastos
{
    public class IngresarGastoDTOs
    {
        public int id_sesion_caja { get; set; }
        public int id_usuario { get; set; }
        public string descripcion { get; set; }
        public decimal monto { get; set; }
        public string observacion { get; set; }
    }
}
