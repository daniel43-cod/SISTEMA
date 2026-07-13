using SISTEMA_FROTEND.DTOs.Cliente;
using System;
using System.Collections.Generic;
using System.Text;

namespace SISTEMA_FROTEND.DTOs.Ventas
{
    public class VentaDTOs
    {
        public int? id_cliente { get; set; }
        public string? nombre_cliente { get; set; }
        public ClienteNuevoDTOs? clienteNuevo { get; set; }
        public int? id_usuario { get; set; }
      //  public decimal monto_pagado { get; set; }
        public string? observacion_pago { get; set; }
        public string origen { get; set; }
        public PagoVentaDTO pago { get; set; }

        public List<DetalleDTOs> detalles { get; set; }
    }
}
