using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Primitives;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;

namespace API_SISTEMA.models
{
    public class RegistroCompras
    {
        [Key]
        [Column("id_compra")]
        public int IdCompra { get; set; }

        [Required]
        [Column("id_proveedor")]
        public int IdProveedor { get; set; }
        
        [Required]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Required]
        [Column("id_estado_compra")]
        public int IdEstadoCompra { get; set; }

        [Required]
        [Column("fecha_ingreso")]
        public DateTime FechaIngreso { get; set; }

        [Required]
        [Column("total_compra")]
        public decimal TotalCompra { get; set; } 

        [Required]
        [Column("saldo_pendiente")]
        public decimal SaldoPendiente { get; set; }

         [Column("observacion", TypeName ="nvarchar(100)")]
        public string? Observacion { get; set; } 
        public Usuario Usuario { get; set; }
        public Proveedores Proveedores { get; set; } 
        public EstadoCompra EstadoCompra { get; set; }

    }
}
