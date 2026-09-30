namespace API_SISTEMA.DTOs.Productos
{
    public class ProductoPresentacionDTO
    {
        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue)]
        public int id_presentacion { get; set; }
        public int unidades_equivalentes { get; set; }
        public decimal precio { get; set; }
    }
}
