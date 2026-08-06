using System;
using System.Collections.Generic;
using System.Text;

namespace SISTEMA_FROTEND.DTOs.Caja
{
    public class RespuetaCierreDTO
    {
        public string mensaje { get; set; } = string.Empty;

        public int id_sesion_caja { get; set; }

        public DateTime fecha_apertura { get; set; }

        public DateTime? fecha_cierre { get; set; }

        public decimal monto_inicial { get; set; }

        public decimal monto_esperado { get; set; }

        public decimal monto_contado { get; set; }

        public decimal diferencia { get; set; }
    }
}
