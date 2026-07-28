using System;
using System.Collections.Generic;
using System.Text;

namespace SISTEMA_FROTEND.DTOs.Caja
{
    internal class RespuestaCajaDTOs
    {
        public string mensaje {  get; set; }=string.Empty;
        public decimal monto_inicial { get; set; }
    }
}
