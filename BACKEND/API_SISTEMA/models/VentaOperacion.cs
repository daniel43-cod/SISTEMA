namespace API_SISTEMA.Models;

// La misma clave no puede registrar una segunda venta, incluso con varias instancias de la API.
public sealed class VentaOperacion
{
    public Guid id_operacion { get; set; }
    public int id_usuario { get; set; }
    public string huella { get; set; } = "";
    public int id_venta { get; set; }
    public Ventas Venta { get; set; } = null!;
}
