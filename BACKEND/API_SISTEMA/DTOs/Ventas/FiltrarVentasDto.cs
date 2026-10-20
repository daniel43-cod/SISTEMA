namespace API_SISTEMA.Dtos.Ventas
{
    public class FiltrarVentasDto
    {
        public int pagina { get; set; } = 1;
        public int tamanoPagina { get; set; } = 50;
        public DateTime? desde { get; set;  }
        public DateTime? hasta { get; set; }

    }
}
