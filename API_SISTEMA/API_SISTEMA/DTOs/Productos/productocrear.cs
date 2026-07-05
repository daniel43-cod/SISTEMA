using API_SISTEMA.models;

namespace API_SISTEMA.DTOs.Productos
{
    public class productocrear
    {
        public string codigo_barra { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int id_categoria { get; set; }
        public decimal precio_compra { get; set; }
        public int stock { get; set; }
        public int stock_minimo { get; set; }
        public decimal impuesto { get; set; }
    

        public List<ProductoPresentacionDTO> presentaciones { get; set; } = new();
        

    }
}
