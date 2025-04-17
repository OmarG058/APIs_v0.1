using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel.DataAnnotations;

namespace APIs_v0._1.Models
{
    public class Cotizacion
    {
        [Key]
        public int IdCotizacion { get; set; }
        [Required]
        public DateTime Fecha { get; set; }
        [Required]
        public int IdSeguro { get; set; }
        [Required]
        public string IdAuto { get; set; }

        [Required]
        public decimal CostoTotal { get; set; }
        [Required]
        public int IdCliente { get; set; }

    }
}
