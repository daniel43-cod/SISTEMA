using System;
using System.Collections.Generic;
using System.Text;

namespace SISTEMA_FROTEND.DTOs.Ventas
{
   
        public class CrearVentaDTO
        {
            public int id_cliente { get; set; }

            public CrearClienteVentaDTO? clienteNuevo { get; set; }

            public string? origen { get; set; }

            public string? observacion { get; set; }

            public List<CrearDetalleVentaDTO> detalles { get; set; } = new();

            public CrearPagoVentaDTO? pago { get; set; }
        }

        public class CrearClienteVentaDTO
        {
            public string nombre { get; set; } = string.Empty;
            public string? apellido { get; set; }
            public string? nit { get; set; }
            public string? dpi { get; set; }
            public string? telefono { get; set; }
            public string? correo_electronico { get; set; }
            public string? direccion { get; set; }
        }

        public class CrearDetalleVentaDTO
        {
            public int id_producto { get; set; }
            public int id_producto_presentacion { get; set; }
            public int cantidad { get; set; }
            public decimal descuento { get; set; }
        }

        public class CrearPagoVentaDTO
        {
            public decimal monto { get; set; }
            public string? metodo_pago { get; set; }
            public string? observacion { get; set; }
        }
    
}
