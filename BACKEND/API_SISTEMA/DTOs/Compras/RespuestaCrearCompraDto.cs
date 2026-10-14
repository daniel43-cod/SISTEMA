using System.Text.Json.Serialization;

namespace API_SISTEMA.Dtos.Compras;

public sealed class RespuestaCrearCompraDto
{
    [JsonPropertyName("mensaje")]
    public string Mensaje { get; init; } = "Compra registrada correctamente";
    [JsonPropertyName("id_compra")]
    public int IdCompra { get; init; }
    [JsonPropertyName("total_compra")]
    public decimal TotalCompra { get; init; }
    [JsonPropertyName("saldo_pendiente")]
    public decimal SaldoPendiente { get; init; }
    [JsonPropertyName("productos")]
    public List<ProductoCompraActualizadoDto> Productos { get; init; } = new();
}

public sealed class ProductoCompraActualizadoDto
{
    public int IdProducto { get; init; }
    public int Stock { get; init; }
    public decimal PrecioPromedioCompra { get; init; }
}
