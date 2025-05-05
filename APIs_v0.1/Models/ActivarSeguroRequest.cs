namespace APIs_v0._1.Models
{
    public class ActivarSeguroRequest
    {
        public string IdVenta { get; set; }
        public int IdSeguro { get; set; }
        public decimal Monto { get; set; }
        public string MetodoPago { get; set; }

    }
}
