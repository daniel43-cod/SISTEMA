using API_SISTEMA.DTOs.Ventas;

namespace API_SISTEMA.DTOs.Compras
{
    public class RegistroComprasDTO
    {
        public int id_usuario { get; set; }
        public int id_empresa { get; set; }
        public int id_estado_compra {  get; set; }
        public List<DetalleCompraDTOs> detalle_compra { get; set; } = new();

    }
}
