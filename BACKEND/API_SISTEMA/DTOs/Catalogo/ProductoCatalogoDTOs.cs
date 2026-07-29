namespace API_SISTEMA.DTOs.Catalogo
{
    public class ProductoCatalogoDTOs
    {
        public int id_producto { get; set; }

        public string nombre { get; set; } = string.Empty;

        public string? imagen { get; set; }

        public int stock { get; set; }

        public List<PresentacionCatalogoDTOs> presentaciones { get; set; } = new();
    }
}
