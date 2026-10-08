namespace API_SISTEMA.Dtos.Productos;

public class ListarPresentacionProductoDto
{
    public int id_producto_presentacion { get; set; }
    public int id_producto { get; set; }
    public int id_presentacion { get; set; }
    public string? descripcion { get; set; }
    public int unidades_equivalentes { get; set; }
    public decimal precio { get; set; }
    public bool estado { get; set; }
}
