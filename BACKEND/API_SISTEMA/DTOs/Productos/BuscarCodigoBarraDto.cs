namespace API_SISTEMA.Dtos.Productos
{
    public class BuscarCodigoBarraDto
    {

        public int id_producto { get; set; }
        public string codigo_barra { get; set; } = string.Empty;
        public string nombre_producto { get; set; } = string.Empty;
        public int stock { get; set; }

        public List<PresentacionCodigoBarraDto> presentaciones { get; set; }
            = new();
    }
    
}