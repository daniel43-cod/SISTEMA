namespace API_SISTEMA.Dtos.MovimientoCaja;

// Los totales corresponden a toda la sesion; la lista contiene solo los ultimos 100 movimientos.
public sealed class ResumenMovimientosCajaDto
{
    public int id_sesion_caja { get; set; }
    public decimal monto_inicial { get; set; }
    public decimal total_entradas { get; set; }
    public decimal total_salidas { get; set; }
    public decimal saldo_esperado { get; set; }

    // Permite indicar en pantalla cuando hay mas movimientos que los incluidos en la lista.
    public int total_movimientos { get; set; }
    public List<ListarMovimientoCajaDto> movimientos { get; set; } = new();
}
