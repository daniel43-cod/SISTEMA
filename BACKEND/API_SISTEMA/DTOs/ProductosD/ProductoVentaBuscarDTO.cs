namespace API_SISTEMA.DTOs.Productos
{
    public class ProductoVentaBuscarDTO
    {
        public int id_producto { get; set; }
        public int id_producto_presentacion { get; set; }

        public string nombre_producto { get; set; }
        public string presentacion { get; set; }

        //  public string nombreMostrar => $"{presentacion} - {nombre_producto}";
       // public string nombre { get; set; }
        public int unidades_equivalentes { get; set; }
        public decimal precio { get; set; }
        public int stock { get; set; }
    }

}

