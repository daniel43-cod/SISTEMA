namespace API_SISTEMA.DTOs.Catalogo
{
    public class PresentacionCatalogoDTOs
    {
        public int id_producto_presentacion { get; set; }

        public string presentacion { get; set; } = string.Empty;

        public int unidades_equivalentes { get; set; }

        public decimal precio { get; set; }
    }
}
