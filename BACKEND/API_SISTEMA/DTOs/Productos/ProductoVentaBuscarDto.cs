namespace API_SISTEMA.Dtos.Productos
{
    public class ProductoVentaBuscarDto
    {
        public int id_producto { get; set; }
        public int id_producto_presentacion { get; set; }

        public required string nombre_producto { get; set; }
        public required string presentacion { get; set; }

        //  public string nombreMostrar => $"{presentacion} - {nombre_producto}";
       // public string nombre { get; set; }
        public int unidades_equivalentes { get; set; }
        public decimal precio { get; set; }
        public int stock { get; set; }
    }

}

