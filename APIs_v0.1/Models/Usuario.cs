using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace APIs_v0._1.Models
{
    public class Usuario
    {
        [Key]
        public int? IdUsuario { get; set; }

        [Required, EmailAddress]
        public string Correo { get; set; }
        [Required]
        public string Contrasenia { get; set; }
        [Required]
        public DateTime FechaRegistro { get; set; }

        [ForeignKey("IdTipo")]
        public int IdTipo { get; set; }
    }
}
