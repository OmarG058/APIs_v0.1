using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace APIs_v0._1.Models
{
    public class Cliente
    {
        [Key]
        public int? IdCliente { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(100)]
        public string Apellidos { get; set; }

        [Required]
        [StringLength(10)]
        public string Telefono { get; set; }

        [Required]
        [StringLength(18)]
        public string Curp { get; set; }

        [Required]
        [StringLength(13)]
        public string RFC { get; set; }

        [Required]
        [ForeignKey("IdUsuario")]
        public int IdUsuario { get; set; }

        [Required]
        [StringLength(100)]
        public string Colonia { get; set; }

        [Required]
        [StringLength(100)]
        public string Calle { get; set; }

        [Required]
        [StringLength(100)]
        public string Ciudad { get; set; }

        [Required]
        [StringLength(100)]
        public string Estado { get; set; }

        [Required]
        [StringLength(10)]
        public string CodigoPostal { get; set; }

    }
}
