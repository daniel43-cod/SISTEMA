namespace API_SISTEMA.DTOs.Compras
{
    public class ListarComprasDTOs
    {
        public int id_compra { get; set; }
        public int  id_usuario { get; set; }
        public int id_empresa { get; set; }
        public int id_estado_compra { get; set; }
        public DateTime fecha_ingreso { get; set; }
        public decimal total_compra { get; set; }


    }
}
