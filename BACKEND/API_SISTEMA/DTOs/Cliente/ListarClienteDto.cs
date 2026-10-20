namespace API_SISTEMA.Dtos.Cliente;

// Índice para búsqueda local: nunca incluye NIT, DPI, teléfono o dirección.
public sealed class ListarClienteDto
{
    public int id_cliente { get; set; }
    public string nombre { get; set; } = string.Empty;
}
