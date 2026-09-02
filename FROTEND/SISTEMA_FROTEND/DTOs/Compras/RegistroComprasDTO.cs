using System;
using System.Collections.Generic;
using System.Text;

namespace SISTEMA_FROTEND.DTOs.Compras
{
    public class RegistroComprasDTO
    {
        public int id_empresa { get; set; }
        public string observacion { get; set; }
        public decimal monto_pagado { get; set; }
        public List<DetalleCompraDTOs> detalle_compra { get; set; } = new();

    }
}
