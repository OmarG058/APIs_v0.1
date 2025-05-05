namespace APIs_v0._1.Models
{
    public class AgregarPagoAutoRequest
    {
        public string IdVenta { get; set; } // MongoDB Id
        public decimal Monto { get; set; }
        public string Metodo { get; set; } // Efectivo, transferencia, etc.
    }
}
