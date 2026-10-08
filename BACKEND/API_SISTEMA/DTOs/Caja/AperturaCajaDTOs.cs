using System.ComponentModel.DataAnnotations;
namespace API_SISTEMA.DTOs.Caja;

public class AperturaCajaDTOs
{
    // Limites coherentes con decimal(10,2); el servicio valida tambien la escala.
    [Range(1, int.MaxValue)] public int id_caja { get; set; }
    [Range(typeof(decimal), "0", "99999999.99")] public decimal monto_inicial { get; set; }
    [StringLength(100)] public string? observacion { get; set; }
}
