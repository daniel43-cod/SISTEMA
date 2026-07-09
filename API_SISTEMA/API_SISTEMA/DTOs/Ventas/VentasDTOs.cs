using API_SISTEMA.DTOs.Cliente;
using API_SISTEMA.models;
using System.ComponentModel.DataAnnotations;

namespace API_SISTEMA.DTOs.Ventas
{
    public class VentasDTOs
    {
        public int id_cliente { get; set; }
        public string nombre_cliente { get; set; }
        public ClienteDTOs? clienteNuevo { get; set; }
        public int? id_usuario { get; set; }
        public decimal monto_pagado { get; set; }
        public string observacion_pago { get; set; }
        public string origen {  get; set; }
        public PagosDTO pago {  get; set; }
        public List<DetalleDTOs> detalles { get; set; } = new();




    }
}
