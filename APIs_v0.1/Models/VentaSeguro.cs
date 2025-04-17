using MongoDB.Bson.Serialization.Attributes;

namespace APIs_v0._1.Models
{
    public class VentaSeguro
    {
        [BsonElement("Contratado")]
        public bool Contratado { get; set; }

        [BsonElement("TipoSeguro")]     
        public string? TipoSeguro { get; set; }

        [BsonElement("PrecioTotal")]
        public decimal? PrecioTotal { get; set; }

        [BsonElement("SaldoPendiente")]
        public decimal? SaldoPendiente { get; set; }

        [BsonElement("TipoPago")]
        public string? TipoPago { get; set; }

        [BsonElement("PagosSeguro")]
        public List<Pago>? PagosSeguro { get; set; } = new List<Pago>();
    }
}
