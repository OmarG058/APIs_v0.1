using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace APIs_v0._1.Models
{
    public class VentaAuto
    {
        [BsonElement("PrecioTotal")]
        [BsonRequired]
        public decimal PrecioTotal { get; set; }
        [BsonElement("TipoPago")]
        [BsonRequired]
        public string TipoPago { get; set; }
        [BsonElement("SaldoPendiente")]
        public decimal SaldoPendiente  { get; set; }
        [BsonElement("PagosAuto")]
        public List<Pago> PagosAuto { get; set; } = new List<Pago>();
    }
}
