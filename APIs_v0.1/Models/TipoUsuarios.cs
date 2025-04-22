using System.ComponentModel.DataAnnotations;

namespace APIs_v0._1.Models
{
    public class TipoUsuarios
    {
        [Key]
        public int IdTipoUsuario { get; set; }
        public string TipoUsuario { get; set; }
    }
}
