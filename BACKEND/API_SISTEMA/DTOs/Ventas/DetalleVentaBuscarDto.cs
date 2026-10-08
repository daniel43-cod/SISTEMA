namespace API_SISTEMA.Dtos.Ventas;

public class DetalleVentaBuscarDto
    {
        public int id_detalle_venta { get; set; }
        public int id_producto { get; set; }
        public string producto { get; set; }
        public int cantidad { get; set; }
        public decimal precio { get; set; }
        public decimal descuento { get; set; }
        public decimal subtotal { get; set; }
    }
