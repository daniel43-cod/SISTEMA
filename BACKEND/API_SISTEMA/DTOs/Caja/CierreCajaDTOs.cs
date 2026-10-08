using System.ComponentModel.DataAnnotations;
namespace API_SISTEMA.DTOs.Caja;

public class CierreCajaDTOs
{
    // Confirma el turno que el administrador vio; evita cerrar un turno posterior por error.
    [Range(1, int.MaxValue)] public int id_sesion_caja { get; set; }
    [Range(typeof(decimal), "0", "99999999.99")] public decimal monto_contado { get; set; }
    [StringLength(100)] public string? observacion_cierre { get; set; }
}
