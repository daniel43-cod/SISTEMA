using System;
using System.Collections.Generic;
using System.Text;

namespace SISTEMA_FROTEND.DTOs.Caja
{
    public class CierreCaja
    {
        public decimal monto_contado { get; set; }
        public string? observacion_cierre { get; set; }
    }
}
