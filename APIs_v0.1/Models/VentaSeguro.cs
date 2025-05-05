using MongoDB.Bson.Serialization.Attributes;

namespace APIs_v0._1.Models
{
    public class VentaSeguro
    {
        [BsonElement("Estado")]
        public string Estado { get; set; }

        [BsonElement("FechaContratacion")]
        public DateTime FechaContratacion { get; set; }

        [BsonElement("FechaFinalizacion")]
        public DateTime FechaFinalizacion { get; set; }

 
        [BsonElement("PrecioTotal")]
        public decimal PrecioTotal { get; set; }

        [BsonElement("SaldoPendiente")]
        public decimal SaldoPendiente { get; set; }


        [BsonElement("PagosSeguro")]
        public List<Pago>? PagosSeguro { get; set; } = new List<Pago>();
    }
}
