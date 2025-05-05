using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel.DataAnnotations;

namespace APIs_v0._1.Models
{
    public class Cotizacion
    {
        [Key]
        public int? IdCotizacion { get; set; }
        [Required]
        public DateTime Fecha { get; set; }
        
        public int? IdSeguro { get; set; }
        [Required]
        public string IdAuto { get; set; }

        public decimal? CostoTotal { get; set; }
        [Required]
        public int IdCliente { get; set; }
        public string? IdAsiento { get; set; }
        public string? IdColor { get; set; }
        public string? IdRin { get; set; }
        public string TipoPago { get; set; }  //contado // o "Credito"
        public int? PlazoMeses { get; set; } // 12, 24 o 36 (si aplica)
        public decimal? Mensualidad { get; set; } // Si es crédito




    }
}
