namespace API_SISTEMA.Dtos.Catalogo
{
    public class ProductoCatalogoDto
    {
        public int id_producto { get; set; }

        public string nombre { get; set; } = string.Empty;

        public string? imagen { get; set; }

        public int stock { get; set; }

        public List<PresentacionCatalogoDto> presentaciones { get; set; } = new();
    }
}
