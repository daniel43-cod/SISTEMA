using API_SISTEMA.DTOs.Ventas;

namespace API_SISTEMA.DTOs.Compras
{
    public class RegistroComprasDTO
    {
        public int id_empresa { get; set; }
        public string observacion { get; set; } 
        public decimal monto_pagado { get; set; }
        public List<DetalleCompraDTOs> detalle_compra { get; set; } = new();

    }
}
