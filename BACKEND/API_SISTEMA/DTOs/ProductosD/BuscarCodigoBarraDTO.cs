namespace API_SISTEMA.DTOs.ProductosD
{
    public class BuscarCodigoBarraDTO
    {

        public int id_producto { get; set; }
        public string codigo_barra { get; set; } = string.Empty;
        public string nombre_producto { get; set; } = string.Empty;
        public int stock { get; set; }

        public List<PresentacionCodigoBarraDTO> presentaciones { get; set; }
            = new();
    }
    public class PresentacionCodigoBarraDTO
    {
        public int id_producto_presentacion { get; set; }
        public string presentacion { get; set; } = string.Empty;
        public int unidades_equivalentes { get; set; }
        public decimal precio { get; set; }
    }
}