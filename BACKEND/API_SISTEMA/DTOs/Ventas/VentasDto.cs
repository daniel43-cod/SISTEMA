using API_SISTEMA.Dtos.Cliente;
using API_SISTEMA.Dtos.Pagos;
using API_SISTEMA.Models;
using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.Dtos.Ventas
{
    public class VentasDto
    {
        public int id_cliente { get; set; }
     
        public ClienteDto? clienteNuevo { get; set; }
        public int? id_usuario { get; set; }
        public string? observacion_pago { get; set; }
        public string origen {  get; set; }
        public int id_sesion_caja { get; set; }
        public PagosDto pago {  get; set; }
        public List<DetalleDto> detalles { get; set; } = new();




    }
}
