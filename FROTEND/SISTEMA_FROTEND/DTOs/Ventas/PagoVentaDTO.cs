using System;
using System.Collections.Generic;
using System.Text;

namespace SISTEMA_FROTEND.DTOs.Ventas
{
    public  class PagoVentaDTO
    {
        public decimal monto_pagado { get; set; }
        public string? observacion_pago { get; set; }
    }
}
